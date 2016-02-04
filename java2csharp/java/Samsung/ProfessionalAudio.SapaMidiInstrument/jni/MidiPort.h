#pragma once

/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "jack_types.h"
#include "MidiEvent.h"


#include <memory>
#include <jack/midiport.h>
#include <string>


namespace audiosuite
{
/**
 * Wrapper of jack midi port functions
 */
class MidiPort
{
public:

    class Buffer
    {
        friend class MidiPort;

        Buffer(jack_port_t* port);

        jack_port_t* port {nullptr};
        sample_t* raw {nullptr};
        void update(jack_nframes_t time) noexcept;
        jack_nframes_t timeOffset {0};
    public:
        /**
         * raw pointer
         * @retrun sample_t* - pointer to data
         **/
        inline sample_t* getRaw() { return raw; }
        jack_nframes_t getEventCount();
        /**
         * last written event time
         * @return jack_nframes_t - time offset
         **/
        void setTimeOffset(jack_nframes_t offset) { timeOffset = offset; }
        /**
         * reserves data for an event in the buffer
         * @param time - time offset
         * @param dataSize - event data size
         * @return jack_midi_data_t - midi data to fill
         **/
        jack_midi_data_t* reserve(jack_nframes_t time, size_t dataSize);
        /**
         * writes an event data with time offset
         * @param time - time offset
         * @param data - midi data
         * @param dataSize - size of the data
         **/
        void write(jack_nframes_t time, const jack_midi_data_t* data, size_t dataSize);
        /**
         * writes MidiEvent to the buffer with time offset
         * @param time - time offset
         * @param event - MidiEvent
         **/
        void write(const MidiEvent& event);
        /** writes buffer content */
        void write(const Buffer& buffer, jack_nframes_t nframes);
        /**
         * raw jack event
         * @param index - event index
         * @return jack_midi_data_t
         **/
        jack_midi_event_t getRawEvent(uint32_t index);
        /**
         * MidiEvent
         * @param index - event index
         * @return MidiEvent
         **/
        MidiEvent getEvent(uint32_t index);
        void clear();
    };

    MidiPort(jack_client_t* jc, const std::string& name, bool output);

    /** retrievs buffer*/
    MidiPort::Buffer& getBuffer(jack_nframes_t nframes) noexcept;

private:

    jack_port_t* port{nullptr};
    std::unique_ptr<MidiPort::Buffer> buffer;
};

} /* namespace audiosuite */
