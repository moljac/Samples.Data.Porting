package com.bxl.textdatatest;

public final class EscapeSequence {
	
	private static String ESCAPE_CHARACTERS = new String(new byte[] {0x1b, 0x7c});

	static String getString(int selectedPosition) {
		
		switch (selectedPosition) {
		case 0:	// Normal
			return ESCAPE_CHARACTERS + "N";
			
		case 1:	// Font A (12x24)
			return ESCAPE_CHARACTERS + "aM";
			
		case 2:	// Font B (9x17)
			return ESCAPE_CHARACTERS + "bM";
			
		case 3:	// Font C (9x24)
			return ESCAPE_CHARACTERS + "cM";
			
		case 4:	// Left justify
			return ESCAPE_CHARACTERS + "lA";
			
		case 5:	// Center
			return ESCAPE_CHARACTERS + "cA";
			
		case 6:	// Right justify
			return ESCAPE_CHARACTERS + "rA";
			
		case 7:	// Bold
			return ESCAPE_CHARACTERS + "bC";
			
		case 8:	// Disabled bold
			return ESCAPE_CHARACTERS + "!bC";
			
		case 9:	// Underline
			return ESCAPE_CHARACTERS + "uC";
			
		case 10:	// Disabled underline
			return ESCAPE_CHARACTERS + "!uC";
			
		case 11:	// Reverse video
			return ESCAPE_CHARACTERS + "rvC";
			
		case 12:	// Disabled reverse video
			return ESCAPE_CHARACTERS + "!rvC";
			
		case 13:	// Single high and wide
			return ESCAPE_CHARACTERS + "1C";
			
		case 14:	// Double wide
			return ESCAPE_CHARACTERS + "2C";
			
		case 15:	// Double high
			return ESCAPE_CHARACTERS + "3C";
			
		case 16:	// Double high and wide
			return ESCAPE_CHARACTERS + "4C";
			
		case 17:	// Scale 1 time horizontally
			return ESCAPE_CHARACTERS + "1hC";
			
		case 18:	// Scale 2 times horizontally
			return ESCAPE_CHARACTERS + "2hC";
			
		case 19:	// Scale 3 times horizontally
			return ESCAPE_CHARACTERS + "3hC";
			
		case 20:	// Scale 4 times horizontally
			return ESCAPE_CHARACTERS + "4hC";
			
		case 21:	// Scale 5 times horizontally
			return ESCAPE_CHARACTERS + "5hC";
			
		case 22:	// Scale 6 times horizontally
			return ESCAPE_CHARACTERS + "6hC";
			
		case 23:	// Scale 7 times horizontally
			return ESCAPE_CHARACTERS + "7hC";
			
		case 24:	// Scale 8 times horizontally
			return ESCAPE_CHARACTERS + "8hC";
			
		case 25:	// Scale 1 time vertically
			return ESCAPE_CHARACTERS + "1vC";
			
		case 26:	// Scale 2 times vertically
			return ESCAPE_CHARACTERS + "2vC";
			
		case 27:	// Scale 3 times vertically
			return ESCAPE_CHARACTERS + "3vC";
			
		case 28:	// Scale 4 times vertically
			return ESCAPE_CHARACTERS + "4vC";
			
		case 29:	// Scale 5 times vertically
			return ESCAPE_CHARACTERS + "5vC";
			
		case 30:	// Scale 6 times vertically
			return ESCAPE_CHARACTERS + "6vC";
			
		case 31:	// Scale 7 times vertically
			return ESCAPE_CHARACTERS + "7vC";
			
		case 32:	// Scale 8 times vertically
			return ESCAPE_CHARACTERS + "8vC";
			
			default:
				return "";
		}
	}
}
