package com.samsung.android.sdk.professionalaudio.widgets;

class NotActivityException extends Exception {
    /**
     * 
     */
    private static final long serialVersionUID = -6125350060493181620L;
    private static final String MSG = "Provided context is not an Activity";

    @Override
    public String getMessage() {
        return MSG;
    }
}
