#pragma once

/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */


#include <cmath>
#include <string>
#include <jack/midiport.h>
#include <stdexcept>
#include <sstream>

namespace audiosuite
{

struct MidiEvent {
    /* we are only interest int those two */
    static const uint32_t NOTE_ON {0x90};
    static const uint32_t NOTE_OFF {0x80};
    static const uint32_t UNSUPPORTED {0x00};

    /* data */
    uint32_t time;
    /* 1st byte values */
    uint32_t channel;
    uint32_t type;
    /* 2nd byte value*/
    uint32_t param1;
    /* 3rd byte value*/
    uint32_t param2;
    size_t size;

    static inline MidiEvent fromRaw(char* data);
    static inline MidiEvent fromJackEvent(const jack_midi_event_t& ev);
    static inline float note2freq(int note);
    static inline void writeMidiData(const MidiEvent& event, jack_midi_data_t* buffer);
    static inline jack_midi_event_t toJackEvent(const MidiEvent& event);
    static inline std::string toString(const MidiEvent& event);
} /* optional variable list */;

MidiEvent MidiEvent::fromRaw(char* data)
{
    MidiEvent ev = {0, 0, 0, 0, 0, 0};

    ev.type = data[0] & 0xf0;

    switch (ev.type)
    {
        case NOTE_OFF:
        case NOTE_ON:
            ev.size = 3;
            ev.channel = data[0] & 0x0f;
            ev.param1 = static_cast<uint32_t>(data[1]);
            ev.param2 = static_cast<uint32_t>(data[2]);
            break;
        default:
            ev.type = UNSUPPORTED;
            ev.size = 1;
    }


    return ev;
}

MidiEvent MidiEvent::fromJackEvent(const jack_midi_event_t& ev)
{
    MidiEvent res = fromRaw(reinterpret_cast<char*>(ev.buffer));
    res.time = ev.time;

    return res;
}

float MidiEvent::note2freq(int note)
{
    return 440.0f * std::pow(2.0f, (note - 69.0f) / 12.0f);
}

void MidiEvent::writeMidiData(const MidiEvent& event, jack_midi_data_t* buffer)
{
    switch (event.size)
    {
        case 3:
            buffer[2] = static_cast<jack_midi_data_t>(event.param2);
        case 2:
            buffer[1] = static_cast<jack_midi_data_t>(event.param1);
        case 1:
            buffer[0] = static_cast<jack_midi_data_t>(event.channel | event.type);
            return;
        default:
            throw std::runtime_error("Unsupported type");
    }
}

jack_midi_event_t MidiEvent::toJackEvent(const MidiEvent& event)
{
    jack_midi_event_t jackEvent;
    switch (event.type)
    {
        case NOTE_OFF:
        case NOTE_ON:
            jackEvent.time = event.time;
            jackEvent.size = event.size;
            jackEvent.buffer = new jack_midi_data_t[event.size];
            writeMidiData(event, jackEvent.buffer);
        default:
            throw std::runtime_error("Unsupported type");
    }

    return jackEvent;
}

std::string MidiEvent::toString(const MidiEvent& event)
{
    std::stringstream ss;
    ss << "time=" << event.time << " ch=" << event.channel << " type=" << event.type;
    if (event.size >= 2)
        ss << " param1=" << event.param1;
    if (event.size == 3)
        ss << " param2=" << event.param2;

    return ss.str();
}

} /* audiosuite */

