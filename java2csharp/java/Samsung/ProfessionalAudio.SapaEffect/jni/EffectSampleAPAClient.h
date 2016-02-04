#pragma once

/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <IAPAInterface.h>

#include <memory>


namespace audiosuite
{
namespace sapaeffectsample
{

class EffectSampleJackClient;
class EffectSampleAPAClient : public ::android::IAPAInterface
{
    std::unique_ptr<EffectSampleJackClient> jackClient;

public:
    EffectSampleAPAClient();
    virtual ~EffectSampleAPAClient();

    virtual int init();
    virtual android::IJackClientInterface* getJackClientInterface();
    virtual int request(const char*, const long, const long, size_t&,
            void* const);
    /**
     * Command callback, when send from the application layer
     */
    virtual int sendCommand(const char*);
};

} /* namespace sapaeffectsample */
} /* namespace audiosuite */


DECLARE_APA_INTERFACE(
        ::audiosuite::sapaeffectsample::EffectSampleAPAClient
        )
