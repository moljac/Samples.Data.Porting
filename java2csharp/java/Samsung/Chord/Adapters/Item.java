
package com.samsung.android.sdk.chord.example.adapter;

import android.graphics.Bitmap;

public class Item {

    Bitmap image;

    String text;

    public Item(Bitmap img, String txt) {
        this.image = img;
        this.text = txt;
    }

    public Bitmap getImage() {
        return image;
    }

    public void setImage(Bitmap image) {
        this.image = image;
    }

    public String getText() {
        return text;
    }

    public void setText(String text) {
        this.text = text;
    }

}
