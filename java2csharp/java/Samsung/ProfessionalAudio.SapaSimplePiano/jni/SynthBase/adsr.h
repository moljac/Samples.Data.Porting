//
//  ADRS.h
//
//  License:
//
//  This source code is provided as is, without warranty.
//  You may copy and distribute verbatim copies of this document.
//  You may modify and use this source code to create binary code for your own purposes, free or commercial.
//

#ifndef ADRS_h
#define ADRS_h

#define fadeoutCoef 0.987688f
#define removeCoef 0.707107f

typedef struct _ADSR_PRAM {
	float attackCoef;
	float decayCoef;
	float releaseCoef;
	float sustainLevel;
	float attackBase;
	float decayBase;
	float releaseBase;
}ADSR_PRAM;

class ADSR_Coef {
public:
	ADSR_Coef(void);
	~ADSR_Coef(void);
	void reset(void);
    void setAttackRate(float rate);
    void setDecayRate(float rate);
    void setReleaseRate(float rate);
	void setSustainLevel(float level);
    void setTargetRatioA(float targetRatio);
    void setTargetRatioDR(float targetRatio);
	ADSR_PRAM& getAdsrParm(void);

protected:
	ADSR_PRAM adsr_pram;
	float attackRate;
	float decayRate;
	float releaseRate;	
	float targetRatioA;
	float targetRatioDR;

    float calcCoef(float rate, float targetRatio);
};


class ADSR_PROC {
public:
	ADSR_PROC(void){
		state = env_idle;
		output = 0.0;
	}
	~ADSR_PROC(void) {}

	float process(void);
	float getOutput(void);
	int getState(void);
	void SetStatEnvFadeout(void);
	void SetStatEnvRemove(void);

	void gate(int on);
	void setAdsrParm(ADSR_PRAM &adsr_pram);

	enum envState {
		env_idle = 0,
		env_attack,
		env_decay,
		env_sustain,
		env_release,
		env_fadeout,
		env_remove
	};

protected:
	int state;
	float output;
	ADSR_PRAM adsr_pram;
};

inline float ADSR_PROC::process() {
	switch (state) {
        case env_idle:
            break;
        case env_attack:
			output = adsr_pram.attackBase + output * adsr_pram.attackCoef;
            if (output >= 1.0) {
				output = 1.0;
				state = env_decay;
            }
            break;
        case env_decay:
			output = adsr_pram.decayBase + output * adsr_pram.decayCoef;
            if (output <= adsr_pram.sustainLevel) {
				output = adsr_pram.sustainLevel;
				state = env_sustain;
            }
            break;
        case env_sustain:
            break;
        case env_release:
			output = adsr_pram.releaseBase + output * adsr_pram.releaseCoef;
            if (output <= (float)0.0001f) {
				output = 0.0;
				state = env_idle;
            }
			break;
		case env_fadeout:
			output = output * fadeoutCoef;
			break;
		case env_remove:
			output = output * removeCoef;
			break;
	}
	return output;
}

inline void ADSR_PROC::gate(int gate) {
	if (gate)
		state = env_attack;
	else if (state != env_idle)
		state = env_release;
}

inline int ADSR_PROC::getState() {
    return state;
}

inline void ADSR_PROC::SetStatEnvFadeout() {
	state = env_fadeout;
}

inline void ADSR_PROC::SetStatEnvRemove() {
	state = env_remove;
}


inline float ADSR_PROC::getOutput() {
	return output;
}

#endif
