#ifndef __SOUND_FONT_PARSER_H__
#define __SOUND_FONT_PARSER_H__

#include "SoundFontParserTypes.h"
#include <stdio.h>

class SoundFontParser{

public:
	SoundFontParser();
	~SoundFontParser();

	SoundFontParserResult parsing(char* sf2FilePath);
	int getSampleCount();
	SoundFontParserResult getPresetInfo(int bank, int preset, PresetInfo &infoClass);
	SoundFontParserResult getInstumentInfo(int instrument, InstrumentInfo &infoClass);
	SoundFontParserResult getSampleInfo(int sampleID, SampleInfo &infoClass);


private:
	SoundFontParserResult initParser(char* sf2FilePath);
	SoundFontParserResult isSoundFontFile();
	SoundFontParserResult parseINFO();
	SoundFontParserResult parseSDTA();
	SoundFontParserResult parsePDTA();
	SoundFontParserResult parsePDTA_PHDR(long int nextChunkPos);
	SoundFontParserResult parsePDTA_PBAG(long int nextChunkPos);
	SoundFontParserResult parsePDTA_PMOD(long int nextChunkPos);
	SoundFontParserResult parsePDTA_PGEN(long int nextChunkPos);
	SoundFontParserResult parsePDTA_INST(long int nextChunkPos);
	SoundFontParserResult parsePDTA_IBAG(long int nextChunkPos);
	SoundFontParserResult parsePDTA_IMOD(long int nextChunkPos);
	SoundFontParserResult parsePDTA_IGEN(long int nextChunkPos);
	SoundFontParserResult parsePDTA_SHDR(long int nextChunkPos);

	char*	readFOURCC();
	DWORD	readDWORD();
	WORD	readWORD();
	SHORT	readSHORT();
	BYTE	readBYTE();
	CHAR	readCHAR();
	void	readString(char* buf, DWORD size);
	SFGenerator readSFGenerator();
	SFModulator readSFModulator();
	SFTransform readSFTransform();
	genAmountType readGenAmountType();
	long int readPCM(long int start, long int size, void *buf);

	long int mFileOffset;
	FILE *fp;
	
	char FOURCC[5];
	DWORD	mRIFFTotalSize;
	SF2INFO mInfo;
	SF2SDTA mSDTA;
	SF2PDTA mPDTA;
	
};

#endif