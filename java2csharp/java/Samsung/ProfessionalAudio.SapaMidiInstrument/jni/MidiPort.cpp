/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "MidiPort.h"
#include "log.h"

#include "jack_types.h"

#include <jack/jack.h>

#include <stdexcept>
#include <algorithm>

namespace audiosuite
{

namespace
{

inline jack_port_t* registerPort(jack_client_t* jc,
        const std::string& name, bool output)
{
    return jack_port_register(jc, name.c_str(), JACK_DEFAULT_MIDI_TYPE,
            output ? JackPortIsOutput : JackPortIsInput, 0);
}

}

MidiPort::MidiPort(jack_client_t* jc, const std::string& name,
        bool output) :
    port{registerPort(jc, name, output)}
{
    if (!port)
        throw std::runtime_error("Failed to register ports");

    buffer.reset(new MidiPort::Buffer(port));
}

MidiPort::Buffer& MidiPort::getBuffer(jack_nframes_t nframes) noexcept
{
    buffer->update(nframes);
    return *(buffer.get());
}

MidiPort::Buffer::Buffer(jack_port_t* port) :
    port(port)
{
}

void MidiPort::Buffer::update(jack_nframes_t nframes) noexcept
{
    raw = reinterpret_cast<sample_t *>(
            jack_port_get_buffer(port, nframes)
            );
}

jack_nframes_t MidiPort::Buffer::getEventCount()
{
    return jack_midi_get_event_count(raw);
}

jack_midi_data_t* MidiPort::Buffer::reserve(jack_nframes_t time, size_t dataSize)
{
    auto data = jack_midi_event_reserve(raw, time, dataSize);
    if (data != nullptr)
        return data;
    throw std::runtime_error("error reserving event space");
}

void MidiPort::Buffer::write(jack_nframes_t time, const jack_midi_data_t* data, size_t dataSize)
{
    if (0 != jack_midi_event_write(raw, time + timeOffset, data, dataSize))
        throw std::runtime_error("error writing buffer");
}

void MidiPort::Buffer::write(const MidiEvent& ev)
{
    jack_midi_data_t* midiData = reserve(ev.time, ev.size);
    MidiEvent::writeMidiData(ev, midiData);
}

void MidiPort::Buffer::write(const Buffer& buffer, jack_nframes_t nframes)
{
    sample_t* in = buffer.raw;
    sample_t* out = raw;
    std::copy(in, in + nframes, out);
}

jack_midi_event_t MidiPort::Buffer::getRawEvent(uint32_t index)
{
    jack_midi_event_t ev;
    if (0 != jack_midi_event_get(&ev, raw, index))
    {
        throw std::runtime_error("Buffer is empty. Use getEventCount to check buffer");
    }
    return ev;
}

MidiEvent MidiPort::Buffer::getEvent(uint32_t index)
{
    auto raw = getRawEvent(index);

    MidiEvent event {0, 0, 0, 0, 0, 0};
    event.time = raw.time;
    event.size = raw.size;

    switch (event.size)
    {
        case 3:
            event.param2 = raw.buffer[2];
        case 2:
            event.param1 = raw.buffer[1];
        case 1:
            event.type = raw.buffer[0] & 0xf0;
            event.channel  = raw.buffer[0] & 0x0f;
    }

    return event;
}

void MidiPort::Buffer::clear()
{
    jack_midi_clear_buffer(raw);
}

} /* namespace audiosuite */
