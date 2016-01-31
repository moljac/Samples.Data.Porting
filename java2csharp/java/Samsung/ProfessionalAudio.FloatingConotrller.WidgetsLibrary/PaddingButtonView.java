package com.samsung.android.sdk.professionalaudio.widgets;

import android.content.Context;
import android.graphics.Canvas;
import android.util.AttributeSet;
import android.widget.ImageButton;
import android.widget.LinearLayout;

public class PaddingButtonView extends ImageButton{
    
    private int mFloatingPaddingLeft=-1;
    private int mFloatingPaddingRight=-1;
    private int mFloatingPaddingTop=-1;
    private int mFloatingPaddingBottom=-1;

    public PaddingButtonView(Context context) {
        super(context);
       // init();
    }

    public PaddingButtonView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        //init();
    }

    public PaddingButtonView(Context context, AttributeSet attrs) {
        super(context, attrs);
        //init();
    }

    private void init(){
        if(mFloatingPaddingBottom!=-1 && mFloatingPaddingLeft!=-1 && mFloatingPaddingRight!=-1 && mFloatingPaddingTop!= -1) return ;
        mFloatingPaddingLeft= super.getPaddingLeft();
        mFloatingPaddingRight= super.getPaddingRight();
        mFloatingPaddingTop= super.getPaddingTop();
        mFloatingPaddingBottom = super.getPaddingBottom();
    }
    
    @Override
    protected void onDraw(Canvas canvas) {
        init();
        resetPaddings();
        super.onDraw(canvas);
    }
    
    private void resetPaddings(){
        if(!(getParent() instanceof LinearLayout)) return;
        LinearLayout parent = (LinearLayout)getParent().getParent();
        if(parent.getOrientation()==LinearLayout.HORIZONTAL && parent.getLayoutDirection()==LAYOUT_DIRECTION_LTR){
            super.setPadding(mFloatingPaddingLeft, mFloatingPaddingTop, mFloatingPaddingRight, mFloatingPaddingBottom);
        }else if(parent.getOrientation()==LinearLayout.HORIZONTAL){
            super.setPadding(mFloatingPaddingRight, mFloatingPaddingTop, mFloatingPaddingLeft, mFloatingPaddingBottom);
        }
        if(parent.getOrientation()==LinearLayout.VERTICAL && parent.getLayoutDirection()==LAYOUT_DIRECTION_RTL){
            super.setPadding(mFloatingPaddingBottom, mFloatingPaddingRight, mFloatingPaddingTop, mFloatingPaddingLeft);
        }else if(parent.getOrientation()==LinearLayout.VERTICAL){
            super.setPadding(mFloatingPaddingBottom, mFloatingPaddingLeft, mFloatingPaddingTop, mFloatingPaddingRight);
        }
        invalidate();
    }
}
