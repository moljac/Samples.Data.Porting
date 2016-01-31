package com.samsung.android.sdk.professionalaudio.widgets;

class NonAvailableActivityException extends Exception {

    /**
     * 
     */
    private static final long serialVersionUID = -1452800299632702425L;
    private static final String MSG = "Activity non available. It has been cleared by GC";

    @Override
    public String getMessage() {
        return MSG;
    }

}
