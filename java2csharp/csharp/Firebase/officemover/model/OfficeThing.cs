namespace com.firebase.officemover.model
{

	using JsonIgnore = com.fasterxml.jackson.annotation.JsonIgnore;
	using JsonIgnoreProperties = com.fasterxml.jackson.annotation.JsonIgnoreProperties;
	using JsonProperty = com.fasterxml.jackson.annotation.JsonProperty;

	/// <summary>
	/// @author Jenny Tong (mimming)
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class OfficeThing
	public class OfficeThing
	{

		private static readonly string TAG = typeof(OfficeThing).Name;
		private int top;
		private int left;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty("z-index") private int zIndex = 1;
		private int zIndex = 1;
		private string type;
		private string name;
		private int rotation;

		//Cache variables
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private String key;
		private string key;

		public OfficeThing()
		{
		}

		public override string ToString()
		{
			return "OfficeThing:" + type + "{name:" + name + ",X:" + left + ",Y:" + top + ",zIndex:" + zIndex + "}";
		}

		public virtual string Key
		{
			get
			{
				return key;
			}
			set
			{
				this.key = value;
			}
		}


		public virtual int Top
		{
			get
			{
				return top;
			}
			set
			{
				this.top = value;
			}
		}


		public virtual int Left
		{
			get
			{
				return left;
			}
			set
			{
				this.left = value;
			}
		}


		public virtual int getzIndex()
		{
			return zIndex;
		}

		public virtual void setzIndex(int zIndex)
		{
			this.zIndex = zIndex;
		}

		public virtual string Type
		{
			get
			{
				return type;
			}
			set
			{
				this.type = value;
			}
		}


		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				this.name = value;
			}
		}


		public virtual int Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				if (value % 90 != 0)
				{
					throw new System.ArgumentException("Rotation must be multiple of 90 or 0, not " + value);
				}
    
				if (value > 360)
				{
					value = value - 360;
				}
				this.rotation = value;
			}
		}

	}

}