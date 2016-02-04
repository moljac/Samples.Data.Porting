//
//  ADSR.cpp
//
//  License:
//
//  This source code is provided as is, without warranty.
//  You may copy and distribute verbatim copies of this document.
//  You may modify and use this source code to create binary code for your own purposes, free or commercial.
//

#include "ADSR.h"
#include <math.h>


ADSR_Coef::ADSR_Coef(void) {
	reset();
}

ADSR_Coef::~ADSR_Coef(void) {
}

void ADSR_Coef::reset(void) {
	setTargetRatioA((float)0.3);
	setTargetRatioDR((float)0.0001);
	setAttackRate(0);
    setDecayRate(0);
    setReleaseRate(0);
    setSustainLevel((float)1.0);
}

void ADSR_Coef::setAttackRate(float rate) {
	attackRate = rate;
	adsr_pram.attackCoef = calcCoef(rate, targetRatioA);
	adsr_pram.attackBase = ((float)1.0 + targetRatioA) * ((float)1.0 - adsr_pram.attackCoef);
}

void ADSR_Coef::setDecayRate(float rate) {
	decayRate = rate;
	adsr_pram.decayCoef = calcCoef(rate, targetRatioDR);
	adsr_pram.decayBase = (adsr_pram.sustainLevel - targetRatioDR) * ((float)1.0 - adsr_pram.decayCoef);
}

void ADSR_Coef::setReleaseRate(float rate) {
	releaseRate = rate;
	adsr_pram.releaseCoef = calcCoef(rate, targetRatioDR);
	adsr_pram.releaseBase = - targetRatioDR * ((float)1.0 - adsr_pram.releaseCoef);
}

float ADSR_Coef::calcCoef(float rate, float targetRatio) {
    return (float)exp(-log((1.0 + targetRatio) / targetRatio) / rate);
}

void ADSR_Coef::setSustainLevel(float level) {
	adsr_pram.sustainLevel = level;
	adsr_pram.decayBase = (adsr_pram.sustainLevel - targetRatioDR) * ((float)1.0 - adsr_pram.decayCoef);
}

void ADSR_Coef::setTargetRatioA(float targetRatio) {
    if (targetRatio < (float)0.000000001)
		targetRatio = (float)0.000000001;  // -180 dB
	targetRatioA = targetRatio;
	adsr_pram.attackBase = ((float)1.0 + targetRatioA) * ((float)1.0 - adsr_pram.attackCoef);
}

void ADSR_Coef::setTargetRatioDR(float targetRatio) {
    if (targetRatio < (float)0.000000001)
        targetRatio = (float)0.000000001;  // -180 dB
	targetRatioDR = targetRatio;
	adsr_pram.decayBase = (adsr_pram.sustainLevel - targetRatioDR) * ((float)1.0 - adsr_pram.decayCoef);
	adsr_pram.releaseBase = -targetRatioDR * ((float)1.0 - adsr_pram.releaseCoef);
}

ADSR_PRAM& ADSR_Coef::getAdsrParm(void)
{
	return adsr_pram;
}

void ADSR_PROC::setAdsrParm(ADSR_PRAM &pram)
{
	adsr_pram = pram;
}
