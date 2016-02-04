/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "StereoPort.h"

#include "jack_types.h"

#include <jack/jack.h>

#include <stdexcept>


namespace audiosuite
{

namespace
{

inline jack_port_t* registerSinglePort(jack_client_t* jc,
        const std::string& name, bool output)
{
    return jack_port_register(jc, name.c_str(), JACK_DEFAULT_AUDIO_TYPE,
            output ? JackPortIsOutput : JackPortIsInput, 0);
}

}

StereoPort::StereoPort(jack_client_t* jc, const std::string& name,
        bool output) :
    left{registerSinglePort(jc, name + "_left", output)},
    right{registerSinglePort(jc, name + "_right", output)}
{
    if (!left || !right)
        throw std::runtime_error("Failed to register ports");
}

sample_t* StereoPort::getLeftPortBuffer(jack_nframes_t nframes) noexcept
{
    return reinterpret_cast<sample_t *>(
            jack_port_get_buffer(left, nframes)
            );
}

sample_t* StereoPort::getRightPortBuffer(jack_nframes_t nframes) noexcept
{
    return reinterpret_cast<sample_t *>(
            jack_port_get_buffer(right, nframes)
            );
}

} /* namespace audiosuite */
