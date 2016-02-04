/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <jack/ringbuffer.h>
#include <algorithm>

#include "log.h"
#include "Processors.h"
#include "StereoPort.h"
#include "MidiPort.h"
#include "MidiEvent.h"
#include "Sinus.h"

namespace audiosuite
{
namespace sapamidisample
{
namespace
{
const size_t MIDI_BUFFER_SIZE {128};
} /*  */

MidiProcessor::EventQueue::EventQueue() :
    buffer{jack_ringbuffer_create(MIDI_BUFFER_SIZE * sizeof(MidiEvent)),
            [](jack_ringbuffer_t* buf)
            {
                if (buf == nullptr) return;
                jack_ringbuffer_free(buf);
                buf = nullptr;
            }}
{
}

MidiProcessor::EventQueue::~EventQueue()
{
}

void MidiProcessor::processEvent(const MidiEvent& ev)
{
    switch (ev.type)
    {
        case MidiEvent::NOTE_ON:
            onNoteOn(ev.param1, ev.param2);
            break;
        case MidiEvent::NOTE_OFF:
            onNoteOff(ev.param1, ev.param2);
            break;
    }
}

void MidiProcessor::EventQueue::push(const MidiEvent& event)
{
    size_t bytes = jack_ringbuffer_write(buffer.get(),
            reinterpret_cast<const char*>(&event), sizeof(event));
    if (bytes != sizeof(MidiEvent))
        throw std::runtime_error("push failed");
}

MidiEvent MidiProcessor::EventQueue::pop()
{
    MidiEvent ev;
    size_t bytes = jack_ringbuffer_read(buffer.get(),
            reinterpret_cast<char*>(&ev), sizeof(ev));

    if (bytes != sizeof(MidiEvent))
        throw std::runtime_error("pop failed");

    return ev;
}

bool MidiProcessor::EventQueue::isEmpty()
{
    return jack_ringbuffer_read_space(buffer.get()) > 0;
}

size_t MidiProcessor::EventQueue::size()
{
    return jack_ringbuffer_read_space(buffer.get()) / sizeof(MidiEvent);
}

void MidiProcessor::EventQueue::clear()
{
    jack_ringbuffer_reset(buffer.get());
}

UserMidiProcessor::UserMidiProcessor(std::shared_ptr<MidiPort> midiOutPort,
        std::shared_ptr<AdditiveGenerator> sines) :
    midiOutPort(midiOutPort),
    sines(sines)
{
}

UserMidiProcessor::~UserMidiProcessor()
{
}

jack_nframes_t UserMidiProcessor::process(jack_nframes_t nframes) noexcept
{
    jack_nframes_t time = 0;
    try
    {
        /* numbe of events in the queue */
        size_t size = queue.size();
        auto& out = midiOutPort->getBuffer(nframes);
        /* forward midi event to output port */

        for (size_t i = 0; i < size; ++i)
        {
            MidiEvent ev = queue.pop();

            if (ev.type == MidiEvent::UNSUPPORTED)
                continue;

            ev.time = time++;
            out.write(ev);
        }

    }
    catch (const std::runtime_error& e)
    {
        LOGE("%s", e.what());
        return -1;
    }

    return time;
}

void UserMidiProcessor::onNoteOff(int note, int /*velocity*/)
{
    /* disable sine wave for a note */
    sines->enable(note, false);
}

void UserMidiProcessor::onNoteOn(int note, int /*velocity*/)
{
    /* enable sine wave for a note */
    sines->enable(note, true);
}

InPortMidiProcessor::InPortMidiProcessor(
        std::shared_ptr<MidiPort> midiInPort,
        std::shared_ptr<MidiPort> midiOutPort,
        std::shared_ptr<AdditiveGenerator> sines) :
    midiInPort(midiInPort),
    midiOutPort(midiOutPort),
    sines(sines)
{
}

InPortMidiProcessor::~InPortMidiProcessor()
{
}

jack_nframes_t InPortMidiProcessor::process(jack_nframes_t nframes) noexcept
{
    jack_nframes_t time = 0;
    try
    {
        auto& in = midiInPort->getBuffer(nframes);
        auto& out = midiOutPort->getBuffer(nframes);

        size_t evCount = in.getEventCount();

        /* forwarding midi event to the output port */
        for (size_t i = 0; i < evCount; ++i)
        {
            auto ev = in.getEvent(i);
            out.write(ev);
            time = ev.time;

            processEvent(ev);
        }
    }
    catch (const std::runtime_error& e)
    {
        LOGE("%s", e.what());
        return -1;
    }

    return time;
}

void InPortMidiProcessor::onNoteOn(int note, int /*velocity*/)
{
    /* enable sine wave for a note */
    sines->enable(note, true);
}

void InPortMidiProcessor::onNoteOff(int note, int /*velocity*/)
{
    /* disable sine wave for a note */
    sines->enable(note, false);
}

AudioProcessor::AudioProcessor(std::shared_ptr<StereoPort> stereoOutPort,
        std::shared_ptr<AdditiveGenerator> sines) :
    stereoOutPort(stereoOutPort),
    sines(sines)
{
}

AudioProcessor::~AudioProcessor()
{
}

bool AudioProcessor::process(jack_nframes_t nframes) noexcept
{
    try
    {
        auto* outL = stereoOutPort->getLeftPortBuffer(nframes);
        auto* outR = stereoOutPort->getRightPortBuffer(nframes);

        /* generate a sinewave and fill the buffer of the left output port */
        std::generate(outL, outL + nframes, *sines);
        /* duplicate sinewave to the right output port */
        std::copy(outL, outL + nframes, outR);
    }
    catch (const std::runtime_error& e)
    {
        LOGE("%s", e.what());
        return false;
    }
    return true;
}

void AudioProcessor::shush()
{
    SLOGD("");
    sines->shush();
}

} /* sapamidisample */
} /* audiosuite */

