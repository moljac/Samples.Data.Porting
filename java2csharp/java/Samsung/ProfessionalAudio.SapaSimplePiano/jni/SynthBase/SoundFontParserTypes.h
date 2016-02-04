#ifndef __SOUND_FONT_PARSER_TYPES_H__
#define __SOUND_FONT_PARSER_TYPES_H__

#include "SoundFontParserList.h"

// setting LOG 
#define ParserLOG printf
#define INFO_STR_LEN		255
#define SAMPLE_NAME_LEN		20
#define INSTRUMENT_NAME_LEN 20
#define PRESET_NAME_LEN		20
#define GENERATOR_LEN		61

typedef unsigned long	DWORD;		// unsigned 32bit
typedef unsigned short	WORD;		// unsigned 16bit
typedef unsigned char	BYTE;		// unsigned  8bit

typedef signed short	SHORT;		// signed 16bit
typedef signed char		CHAR;		// signed  8bit

// Result value enum
enum SoundFontParserResult{
	success = 0,
	fail = -1,
	file_open_failed = -2,
	no_sound_font_file = -3,
	no_info_list = -4,
	no_sdta_list = -5,
	no_pdta_list = -6,
	no_preset = -7,
	no_bank = -8,
	no_instrument = -9,
	no_bank_preset = -10,
	no_sample = -11,
	file_seek_failed = -12,
	file_read_failed = -13

};

// SoundFont 2.x defined generators.
typedef enum _SF2GeneratorEnumerators{
	SPA_startAddrsOffset = 0,
	SPA_endAddrsOffset,
	SPA_startloopAddrsOffset,
	SPA_endloopAddrsOffset,
	SPA_startAddrsCoarseOffset,
	SPA_modLfoToPitch = 5,
	SPA_vibLfoToPitch,
	SPA_modEnvToPitch,
	SPA_initialFilterFc,
	SPA_initialFilterQ,
	SPA_modLfoToFilterFc = 10,
	SPA_modEnvToFilterFc,
	SPA_endAddrsCoarseOffset,
	SPA_modLfoToVolume,
	SPA_unused1,
	SPA_chorusEffectsSend = 15,
	SPA_reverbEffectsSend,
	SPA_pan,
	SPA_unused2,
	SPA_unused3,
	SPA_unused4 = 20,
	SPA_delayModLFO,
	SPA_freqModLFO,
	SPA_delayVibLFO,
	SPA_freqVibLFO,
	SPA_delayModEnv = 25,
	SPA_attackModEnv,
	SPA_holdModEnv,
	SPA_decayModEnv,
	SPA_sustainModEnv,
	SPA_releaseModEnv = 30,
	SPA_keynumToModEnvHold,
	SPA_keynumToModEnvDecay,
	SPA_delayVolEnv,
	SPA_attackVolEnv,
	SPA_holdVolEnv = 35,
	SPA_decayVolEnv,
	SPA_sustainVolEnv,
	SPA_releaseVolEnv,
	SPA_keynumToVolEnvHold,
	SPA_keynumToVolEnvDecay = 40,
	SPA_instrument,
	SPA_reserved1,
	SPA_keyRange,
	SPA_velRange,
	SPA_startloopAddrsCoarseOffset = 45,
	SPA_keynum,
	SPA_velocity,
	SPA_initialAttenuation,
	SPA_reserved2,
	SPA_endloopAddrsCoarseOffset = 50,
	SPA_coarseTune,
	SPA_fineTune,
	SPA_sampleID,
	SPA_sampleModes,
	SPA_reserved3 = 55,
	SPA_scaleTuning,
	SPA_exclusiveClass,
	SPA_overridingRootKey,
	SPA_unused5,
	SPA_endOper = 60,
	SPA_EOE = 61		// End of Enumerators;
}SF2GeneratorEnumerators;

enum SF2SourceEnumeratorControllerPalettes{
	NoController = 0,
	NoteOnVelocity = 2,
	NoteOnKeyNumber = 3,
	PolyPressure = 10,
	ChannelPressure = 13,
	PitchWheel = 14,
	PitchWheelSensitivity = 16,
	Link = 127
};

enum SF2SourceType {
	Linear = 0,
	Concave = 1,
	Convex = 2,
	Switch = 3
};

enum ModulatorTransformEnumerators{
	//Linear = 0,
	AbsoluteValue = 1
};

typedef struct{
	BYTE byLo;
	BYTE byHi;
}rangesType;

typedef union{
	rangesType ranges;
	SHORT shAmount;
	WORD wAmount;
} genAmountType;

typedef unsigned short SFModulator;
typedef unsigned short SFGenerator;
typedef unsigned short SFTransform;

typedef enum _SFSampleLink
{
	monoSample = 1,
	rightSample = 2,
	leftSample = 4,
	linkedSample = 8,
	RomMonoSample = 0x8001,
	RomRightSample = 0x8002,
	RomLeftSample = 0x8004,
	RomLinkedSample = 0x8008
} SFSampleLink;

// <INFO-list Chunk> information
typedef struct _SF2INFO {
	WORD	soundFontVersionMajor;				// ifil major
	WORD	soundFontVersionMinor;				// ifil minor
	char	soundEngine[INFO_STR_LEN];			// isng
	char	bankName[INFO_STR_LEN];				// INAM
	char	romName[INFO_STR_LEN];				// irom
	WORD	romRevisionMajor;					// iver major
	WORD	romRevisionMinor;					// iver minor
	char	creationDate[INFO_STR_LEN];			// ICRD
	char	designerName[INFO_STR_LEN];			// IENG
	char	product[INFO_STR_LEN];				// IPRD
	char	copyRight[INFO_STR_LEN];			// ICOP
	char*	comments;							// ICMT
	char	creationTool[INFO_STR_LEN];			// ISFT
} SF2INFO;

// <sdta-list> chunk
typedef struct _SF2SDTA {
	long int	smplStartPosition;
	DWORD		smplSize;
	long int	sm24StartPosition;
	DWORD		sm24Size;
}SF2SDTA;

typedef struct _sfPresetHeader{
	char achPresetName[PRESET_NAME_LEN];
	WORD wPreset;
	WORD wBank;
	WORD wPresetBagNdx;
	DWORD dwLibrary;
	DWORD dwGenre;
	DWORD dwMorphology;
}sfPresetHeader;

typedef struct _sfPresetBag{
	WORD wGenNdx;
	WORD wModNdx;
}sfPresetBag;

typedef struct _sfModList{
	SFModulator sfModSrcOper;
	SFGenerator sfModDestOper;
	SHORT modAmount;
	SFModulator sfModAmtSrcOper;
	SFTransform sfModTransOper;
}sfModList;

typedef struct _sfGenList{
	//SFGenerator sfGenOper;
	SF2GeneratorEnumerators sfGenOper;
	genAmountType genAmount;
}sfGenList;

typedef struct _sfInst{
	char achInstName[INSTRUMENT_NAME_LEN];
	WORD wInstBagNdx;
}sfInst;

typedef struct _sfInstBag{
	WORD wInstGenNdx;
	WORD wInstModNdx;
}sfInstBag;

typedef struct _sfInstGenList
{
	//SFGenerator sfGenOper;
	SF2GeneratorEnumerators sfGenOper;
	genAmountType genAmount;
}sfInstGenList;

typedef struct _sfSample
{
	// from shdr
	char achSampleName[SAMPLE_NAME_LEN];
	DWORD dwStart;
	DWORD dwEnd;
	DWORD dwStartloop;
	DWORD dwEndloop;
	DWORD dwSampleRate;
	BYTE byOriginalPitch;
	char chPitchCorrection;
	WORD wSampleLink;
	SFSampleLink sfSampleType;
}sfSample;

// <pdta-list> chunk
typedef struct _SF2PDTA{
	SoundFontParserList<sfPresetHeader> phdr;
	SoundFontParserList<sfPresetBag> pbag;
	SoundFontParserList<sfModList> pmod;
	SoundFontParserList<sfGenList> pgen;
	SoundFontParserList<sfInst> inst;
	SoundFontParserList<sfInstBag> ibag;
	SoundFontParserList<sfModList> imod;
	SoundFontParserList<sfInstGenList> igen;
	SoundFontParserList<sfSample> shdr;
}SF2PDTA;

typedef struct _SampleInfo{
	// from IGEN
	BYTE	keyRangeLow;
	BYTE	keyRangeHigh;
	SHORT	pan;
	SHORT	sampleID;
}SampleInfoFromIGEN;

///////////////////////////////////////////////////////////////////////
// For User
//////////////////////////////////////////////////////////////////////
class PresetInstrumentInfo {
public:
	PresetInstrumentInfo();

	char name[INSTRUMENT_NAME_LEN];
	int instrumentID;
	int genCount;
	sfGenList gen[GENERATOR_LEN];
};

class PresetInfo {
public:
	PresetInfo();

	int bank;
	int preset;
	int globalGenCount;
	sfGenList globalGen[GENERATOR_LEN];
	SoundFontParserList<PresetInstrumentInfo> instrumentInfo;
};

class InstrumentSampleInfo{
public:
	InstrumentSampleInfo();

	char sampleName[SAMPLE_NAME_LEN];
	int sampleID;
	int genCount;
	sfGenList gen[GENERATOR_LEN];
};

class InstrumentInfo{
public:
	InstrumentInfo();

	char name[INSTRUMENT_NAME_LEN];
	int instrumentID;
	int globalGenCount;
	sfGenList globalGen[GENERATOR_LEN];
	SoundFontParserList<InstrumentSampleInfo> sampleInfo;
};

class SampleInfo{
public:
	SampleInfo();
	~SampleInfo();

	char name[SAMPLE_NAME_LEN];
	int sampleID;
	int sampleSize;  // pcm byte size
	void* sampleData;  // pcm buffer, alloc/free internally.

	DWORD dwStartloop;
	DWORD dwEndloop;
	DWORD dwSampleRate;
	BYTE byOriginalPitch;
	char chPitchCorrection;
	WORD wSampleLink;
	SFSampleLink sfSampleType;
};

#endif