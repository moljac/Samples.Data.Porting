/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <cmath>
#include <atomic>

#include "jack_types.h"


namespace audiosuite
{

namespace sapaeffectsample
{
/**
 * Functor calculating sample values after applying volume
 */
struct Volume {
    /* data */
    sample_t operator()(sample_t in)
    {
        sample_t out = in * std::pow(10.0f, dB / 10.0f);
        return out;
    }
    float dB {1.0f};
} /* optional variable list */;

}

}
