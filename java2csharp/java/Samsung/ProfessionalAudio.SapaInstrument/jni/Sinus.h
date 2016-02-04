/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <cmath>

#include "jack_types.h"


namespace audiosuite
{
namespace sapainstrumentsample
{

class Sinus
{
    const float step;
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
        step{calcStep(freq, samplerate)}
    {
    }

    sample_t operator()()
    {
        auto sample = std::sin(x);
        x += step;

        if (x >= 2.0f * M_PI)
            x = 0.0f;

        return sample;
    }
};

} /* namespace sapainstrumentsample */
} /* namespace audiosuite */
