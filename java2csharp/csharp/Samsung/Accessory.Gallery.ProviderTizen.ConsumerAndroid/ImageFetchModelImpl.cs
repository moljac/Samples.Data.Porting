using System.Collections.Generic;

/*
 * Copyright (c) 2015 Samsung Electronics Co., Ltd. All rights reserved. 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that 
 * the following conditions are met:
 * 
 *     * Redistributions of source code must retain the above copyright notice, 
 *       this list of conditions and the following disclaimer. 
 *     * Redistributions in binary form must reproduce the above copyright notice, 
 *       this list of conditions and the following disclaimer in the documentation and/or 
 *       other materials provided with the distribution. 
 *     * Neither the name of Samsung Electronics Co., Ltd. nor the names of its contributors may be used to endorse or 
 *       promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

namespace com.samsung.android.sdk.accessory.example.galleryconsumer
{

	using JSONArray = org.json.JSONArray;
	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	public class ImageFetchModelImpl
	{
		public sealed class TBListReqMsg : Model.JsonSerializable
		{
			internal string mMessgaeId = "";
			internal long? mId = -1L;
			public const string ID_Renamed = "offset";

			public TBListReqMsg()
			{
			}

			public TBListReqMsg(long? id)
			{
				mMessgaeId = Model.THUMBNAIL_LIST_REQ;
				mId = id;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object toJSON() throws org.json.JSONException
			public object toJSON()
			{
				JSONObject json = new JSONObject();
				json.put(Model.MSG_ID, mMessgaeId);
				json.put(ID_Renamed, mId);
				return json;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void fromJSON(Object obj) throws org.json.JSONException
			public void fromJSON(object obj)
			{
				JSONObject json = (JSONObject) obj;
				mMessgaeId = json.getString(Model.MSG_ID);
				mId = json.getLong(ID_Renamed);
			}

			public string MessageIdentifier
			{
				get
				{
					return mMessgaeId;
				}
			}

			public long ID
			{
				get
				{
					return mId.Value;
				}
			}
		}

		public sealed class TBListRespMsg : Model.JsonSerializable
		{
			internal string mMessgaeId = "";
			internal string mResult = "";
			internal int mReason = 0;
			internal int mCount = 0;
			public IList<TBModelJson> msgTBList = null;
			public const string COUNT = "count";
			public const string LIST = "list";
			public const string REASON = "reason";
			public const string RESULT = "result";

			public TBListRespMsg()
			{
			}

			public TBListRespMsg(string result, int reason, int count, IList<TBModelJson> TBList)
			{
				mMessgaeId = Model.THUMBNAIL_LIST_RSP;
				mResult = result;
				mReason = reason;
				mCount = count;
				msgTBList = new List<TBModelJson>();
				((List<TBModelJson>)msgTBList).AddRange(TBList);
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object toJSON() throws org.json.JSONException
			public object toJSON()
			{
				JSONObject json = new JSONObject();
				json.put(Model.MSG_ID, mMessgaeId);
				json.put(RESULT, mResult);
				json.put(REASON, mReason);
				json.put(COUNT, mCount);
				JSONArray msgarray = new JSONArray();
				foreach (TBModelJson sms in msgTBList)
				{
					object obj = sms.toJSON();
					msgarray.put(obj);
				}
				json.put(LIST, msgarray);
				return json;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void fromJSON(Object obj) throws org.json.JSONException
			public void fromJSON(object obj)
			{
				JSONObject json = (JSONObject) obj;
				mMessgaeId = json.getString(Model.MSG_ID);
				mResult = json.getString(RESULT);
				mReason = json.getInt(REASON);
				mCount = json.getInt(COUNT);
				JSONArray jsonArray = json.getJSONArray(LIST);
				msgTBList = new List<TBModelJson>();
				for (int i = 0; i < jsonArray.length(); i++)
				{
					JSONObject jsonObjct = (JSONObject) jsonArray.getJSONObject(i);
					TBModelJson sms = new TBModelJson();
					sms.fromJSON(jsonObjct);
					msgTBList.Add(sms);
				}
			}

			public string MessageIdentifier
			{
				get
				{
					return mMessgaeId;
				}
			}

			public int MsgCount
			{
				get
				{
					return mCount;
				}
			}

			public string Result
			{
				get
				{
					return mResult;
				}
			}

			public int Reason
			{
				get
				{
					return mReason;
				}
			}

			public IList<TBModelJson> getmsgTBList()
			{
				return msgTBList;
			}
		}

		public sealed class ImgReqMsg : Model.JsonSerializable
		{
			internal string mMessgaeId = "";
			public const string ID_Renamed = "id";
			public const string WIDTH = "width";
			public const string HEIGHT = "height";
			internal long? mId = -1L;
			internal int mWidth = 0;
			internal int mHeight = 0;

			public ImgReqMsg()
			{
			}

			public ImgReqMsg(long? id, int width, int height)
			{
				mMessgaeId = Model.DOWNSCALE_IMG_REQ;
				mId = id;
				mWidth = width;
				mHeight = height;
			}

			public string MessageIdentifier
			{
				get
				{
					return mMessgaeId;
				}
			}

			public long ID
			{
				get
				{
					return mId.Value;
				}
			}

			public int Width
			{
				get
				{
					return mWidth;
				}
			}

			public int Height
			{
				get
				{
					return mHeight;
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object toJSON() throws org.json.JSONException
			public object toJSON()
			{
				JSONObject json = new JSONObject();
				json.put(Model.MSG_ID, mMessgaeId);
				json.put(ID_Renamed, mId);
				json.put(WIDTH, mWidth);
				json.put(HEIGHT, mHeight);
				return json;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void fromJSON(Object obj) throws org.json.JSONException
			public void fromJSON(object obj)
			{
				JSONObject json = (JSONObject) obj;
				mMessgaeId = json.getString(Model.MSG_ID);
				mId = json.getLong(ID_Renamed);
				mWidth = json.getInt(WIDTH);
				mHeight = json.getInt(HEIGHT);
			}
		}

		public sealed class ImgRespMsg : Model.JsonSerializable
		{
			internal string mMessgaeId = "";
			internal string mResult = "";
			internal int mReason = 0;
			internal TBModelJson mDownscaledImg = null;
			public const string RESULT = "result";
			public const string REASON = "reason";
			public const string IMAGE = "image";

			public ImgRespMsg()
			{
			}

			public ImgRespMsg(string result, int reason, TBModelJson img)
			{
				mMessgaeId = Model.DOWNSCALE_IMG_RSP;
				mResult = result;
				mReason = reason;
				mDownscaledImg = img;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object toJSON() throws org.json.JSONException
			public object toJSON()
			{
				JSONObject json = new JSONObject();
				json.put(Model.MSG_ID, mMessgaeId);
				json.put(RESULT, mResult);
				json.put(REASON, mReason);
				json.put(IMAGE, (JSONObject) mDownscaledImg.toJSON());
				return json;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void fromJSON(Object obj) throws org.json.JSONException
			public void fromJSON(object obj)
			{
				JSONObject json = (JSONObject) obj;
				mMessgaeId = json.getString(Model.MSG_ID);
				mResult = json.getString(RESULT);
				mReason = json.getInt(REASON);
				JSONObject jobj = json.getJSONObject(IMAGE);
				mDownscaledImg = new TBModelJson();
				mDownscaledImg.fromJSON(jobj);
			}

			public string MessageIdentifier
			{
				get
				{
					return mMessgaeId;
				}
			}

			public string Result
			{
				get
				{
					return mResult;
				}
			}

			public int Reason
			{
				get
				{
					return mReason;
				}
			}

			public TBModelJson DownscaledImg
			{
				get
				{
					return mDownscaledImg;
				}
			}
		}

		public sealed class TBModelJson : Model.JsonSerializable
		{
			public string Data
			{
				get
				{
					return mData;
				}
			}

			public string Name
			{
				get
				{
					return mName;
				}
			}

			public int Width
			{
				get
				{
					return mWidth;
				}
			}

			public int Height
			{
				get
				{
					return mHeight;
				}
			}

			public long Size
			{
				get
				{
					return mSize;
				}
			}

			public long Id
			{
				get
				{
					return mId;
				}
			}

			public const string ID = "id";
			public const string DATA = "image";
			public const string SIZE = "size";
			public const string NAME = "name";
			public const string WIDTH = "width";
			public const string HEIGHT = "height";
			internal long mId = -1L;
			internal string mData = "";
			internal long mSize = 0L;
			internal string mName = "";
			internal int mWidth = 0;
			internal int mHeight = 0;

			public TBModelJson()
			{
			};

			public TBModelJson(long id, string name, string data, long size, int width, int height) : base()
			{
				mId = id;
				mName = name;
				mData = data;
				mWidth = width;
				mHeight = height;
				mSize = size;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void fromJSON(Object jsonObj) throws org.json.JSONException
			public void fromJSON(object jsonObj)
			{
				JSONObject json = (JSONObject) jsonObj;
				mId = json.getLong(ID);
				mData = json.getString(DATA);
				mName = json.getString(NAME);
				mSize = json.getLong(SIZE);
				mHeight = json.getInt(HEIGHT);
				mWidth = json.getInt(WIDTH);
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object toJSON() throws org.json.JSONException
			public object toJSON()
			{
				JSONObject json = new JSONObject();
				json.put(ID, mId);
				json.put(NAME, mName);
				json.put(DATA, mData);
				json.put(SIZE, mSize);
				json.put(WIDTH, mWidth);
				json.put(HEIGHT, mHeight);
				return json;
			}
		}
	}

}