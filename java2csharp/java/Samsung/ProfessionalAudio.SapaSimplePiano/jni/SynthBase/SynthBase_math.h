#ifndef SYSTH_BASE__MATH_H
#define SYSTH_BASE__MATH_H

#define SAMPLE_RATE 48000.0f
#define ISAMPLERATE (1.0f/SAMPLE_RATE)

#define NOTE_RANGE_NEG -127
#define NOTE_RANGE_POG 127

#define PITCH_RANGE_CENTER (0x2000>>6)

#define PAN_LEFT -50
#define PAN_RIGHT 50
#define PAN_SIZE (PAN_RIGHT - PAN_LEFT + 1 ) // +1 center value : 0
#define ATTENUATION_SIZE ((144+1) * 10 ) // +1 center value : 0

#define REAL_RANGE_NEG -10
#define REAL_RANGE_POS 10

#define RESOLUTION 10.0

extern const float EXP_T[(NOTE_RANGE_POG - NOTE_RANGE_NEG)+1];
extern const float POW_T[NOTE_RANGE_POG][REAL_RANGE_POS*2+1];
extern const float EXP_CAL0_T[NOTE_RANGE_POG][REAL_RANGE_POS];
extern const float EXP_CAL1_T[NOTE_RANGE_POG][REAL_RANGE_POS];
extern const float PITCH_WHEEL_T[256];
extern const float pan_tab[PAN_SIZE];
extern const float Attenuation_tab[ATTENUATION_SIZE];
void pan_gain_ref(int c, float &l, float &r );

#endif