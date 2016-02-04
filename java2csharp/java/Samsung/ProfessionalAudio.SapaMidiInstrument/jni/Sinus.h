#pragma once

/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <cmath>
#include <iterator>
#include <unordered_map>
#include <atomic>

#include "jack_types.h"
#include "MidiEvent.h"

namespace audiosuite
{
namespace sapamidisample
{

class ASREnvelope
{
    static const uint32_t IDLE{0};
    static const uint32_t ATTACK{1};
    static const uint32_t SUSTAIN{2};
    static const uint32_t RELEASE{3};
    static constexpr uint32_t ATTACK_TIME_MILLIS{5};
    static constexpr uint32_t RELEASE_TIME_MILLIS{5};
    static constexpr sample_t MAX_VALUE{0.25f};
    const sample_t attackDelta;
    const sample_t releaseDelta;
public:
    ASREnvelope(uint32_t sampleRate) :
        attackDelta{MAX_VALUE / (sampleRate * (ATTACK_TIME_MILLIS * 0.001f))},
        releaseDelta{MAX_VALUE / (sampleRate * (RELEASE_TIME_MILLIS * 0.001f))}
    {

    }
    virtual ~ASREnvelope(){}
    inline void activate(){state = ATTACK;}
    inline void deactivate(){state = RELEASE;}

    sample_t operator()()
    {
        switch (state)
        {
            case IDLE:
                value = 0.0f;
                break;
            case ATTACK:
                value += attackDelta;
                if (value >= MAX_VALUE)
                {
                    value = MAX_VALUE;
                    state = SUSTAIN;
                }
                break;
            case RELEASE:
                value -= releaseDelta;
                if (value <= 0.0f)
                {
                    value = 0.0f;
                    state = IDLE;
                }
                break;
            case SUSTAIN:
                break;
        }
        return value;
    }

private:
    /* data */
    sample_t value{0.0f};
    uint32_t state{IDLE};
};

class Sinus
{
    ASREnvelope envelope;
    const float step;
    float freq;
    float x{0};

    static inline float calcStep(float freq, unsigned samplerate)
    {
        return std::abs(static_cast<float>(
                2.0 * M_PI * static_cast<double>(freq)
                / static_cast<double>(samplerate)
                ));
    }

public:
    Sinus(float freq, unsigned int samplerate) :
        envelope{samplerate},
        step{calcStep(freq, samplerate)},
        freq{freq}
    {
    }

    void enable(bool on)
    {
        on ? envelope.activate() : envelope.deactivate();
    }

    sample_t operator()()
    {
        auto sample = std::sin(x) * envelope();
        x += step;

        if (x >= 2.0f * M_PI)
            x = 0.0f;

        return sample;
    }
};

class AdditiveGenerator
{
    unsigned int samplerate;
    /* shared_ptr to enable copy constructor */
    std::shared_ptr<
        std::unordered_map<int, std::unique_ptr<Sinus>>> sines;
public:
    AdditiveGenerator(unsigned int samplerate) :
        samplerate(samplerate),
        sines(new std::unordered_map<int, std::unique_ptr<Sinus>>())
    {}

    /**
     * Add sinus with note frequency.
     *
     * @param note - midi note id
     */
    void addNote(int note)
    {
        /* adding Sinus with note frequency */
        if (sines->find(note) == std::end(*sines))
        {
            float freq = MidiEvent::note2freq(note);
            sines->emplace(note, std::unique_ptr<Sinus>(new Sinus(freq, samplerate)));
        }
    }
    /**
     * Enable signal for a note
     *
     * @param note - midi note id
     * @param on - enable/disable
     */
    void enable(int note, bool on)
    {
        /* enabling note component for sines sum */
        auto sin = sines->find(note);
        if (sin != std::end(*sines))
            sin->second->enable(on);
    }
    void shush()
    {
        for (auto& el : *sines)
            (*el.second).enable(false);
    }
    sample_t operator()()
    {
        /* summing sinuses */
        sample_t result = 0;
        for (auto& el : *sines)
        {
            result += (*el.second)();
        }

        return result;
    }
};

} /* namespace sapamidisample */
} /* namespace audiosuite */
