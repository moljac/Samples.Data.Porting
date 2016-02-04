#pragma once

/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <memory>
#include <functional>
#include <jack/ringbuffer.h>

#include "log.h"
#include "jack_types.h"
#include "MidiEvent.h"

namespace audiosuite
{

class StereoPort;
class MidiPort;

namespace sapamidisample
{

class AdditiveGenerator;
/**
 * Generic midi processor with midi event queue.
 **/
class MidiProcessor
{
    class EventQueue
    {
    public:
        EventQueue();
        ~EventQueue();
        void push(const MidiEvent& event);
        MidiEvent pop();
        bool isEmpty();
        size_t size();
        void clear();
    private:
        std::unique_ptr<jack_ringbuffer_t,
            std::function<void(jack_ringbuffer_t*)>> buffer;
    };

public:
    virtual ~MidiProcessor(){}
    /**
     * Process procedure.
     *
     * @param nframes - frames from jack process callback
     * @return jack_nframes_t - the last event time offset, -1 if error
     */
    virtual jack_nframes_t process(jack_nframes_t nframes) noexcept = 0;
    /**
     * Push event to the queue.
     *
     * @param raw - midi event in some form
     * @param converted - converter to the MidiEven
     */
    template<typename Type>
    void pushEvent(Type raw, std::function<MidiEvent(Type)> converter);
protected:
    MidiProcessor(){}
    void processEvent(const MidiEvent& event);
    /**
     * Note on event callback.
     *
     * @param note - note value
     * @param valocity - note velocity
     */
    virtual void onNoteOn(int /*note*/, int /*velocity*/) {}
    /** As above just for note off. */
    virtual void onNoteOff(int /*note*/, int /*velocity*/) {}

    MidiProcessor::EventQueue queue;
};

template<typename Type>
void MidiProcessor::pushEvent(Type raw, std::function<MidiEvent(Type)> converter)
{
    MidiEvent ev = converter(std::move(raw));

    processEvent(ev);

    queue.push(ev);
}
/**
 * Processor of midi events comming from a user.
 */
class UserMidiProcessor : public MidiProcessor
{
public:
    UserMidiProcessor(
            std::shared_ptr<MidiPort> midiOutPort,
            std::shared_ptr<AdditiveGenerator> sines);
    virtual ~UserMidiProcessor();
    virtual jack_nframes_t process(jack_nframes_t nframes) noexcept;
protected:
    virtual void onNoteOn(int note, int velocity);
    virtual void onNoteOff(int note, int velocity);
private:
    /* data */
    std::shared_ptr<StereoPort> stereoOutPort;
    std::shared_ptr<MidiPort> midiOutPort;
    /** sine waves generator */
    std::shared_ptr<AdditiveGenerator> sines;
} /* optional variable list */;

/**
 * Processor of midi events readed from the input midi port.
 * In this example it only passes through an event to the output port.
 */
class InPortMidiProcessor : public MidiProcessor
{
public:
    InPortMidiProcessor(
            std::shared_ptr<MidiPort> midiInPort,
            std::shared_ptr<MidiPort> midiOutPort,
            std::shared_ptr<AdditiveGenerator> sines);
    virtual ~InPortMidiProcessor();
    virtual jack_nframes_t process(jack_nframes_t nframes) noexcept;
protected:
    virtual void onNoteOn(int note, int /*velocity*/);
    virtual void onNoteOff(int note, int /*velocity*/);
private:
    /* data */
    std::shared_ptr<MidiPort> midiInPort;
    std::shared_ptr<MidiPort> midiOutPort;
    /** sine waves generator */
    std::shared_ptr<AdditiveGenerator> sines;
} /* optional variable list */;

class AudioProcessor
{
public:
    AudioProcessor(
            std::shared_ptr<StereoPort> stereoOutPort,
            std::shared_ptr<AdditiveGenerator> sines);
    virtual ~AudioProcessor();
    bool process(jack_nframes_t nframes) noexcept;
    void shush();
private:
    std::shared_ptr<StereoPort> stereoOutPort;
    std::shared_ptr<AdditiveGenerator> sines;
};

} /* sapamidisample */
} /* audiosuite */
