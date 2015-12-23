namespace com.bxl.postest
{

	using JposConst = jpos.JposConst;
	using POSPrinter = jpos.POSPrinter;
	using POSPrinterConst = jpos.POSPrinterConst;
	using DirectIOEvent = jpos.events.DirectIOEvent;
	using DirectIOListener = jpos.events.DirectIOListener;
	using ErrorEvent = jpos.events.ErrorEvent;
	using ErrorListener = jpos.events.ErrorListener;
	using OutputCompleteEvent = jpos.events.OutputCompleteEvent;
	using OutputCompleteListener = jpos.events.OutputCompleteListener;
	using StatusUpdateEvent = jpos.events.StatusUpdateEvent;
	using StatusUpdateListener = jpos.events.StatusUpdateListener;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using Layout = android.text.Layout;
	using TextView = android.widget.TextView;

	public abstract class POSPrinterFragment : Fragment, StatusUpdateListener, OutputCompleteListener, ErrorListener, DirectIOListener
	{

		protected internal POSPrinter posPrinter;

		protected internal TextView deviceMessagesTextView;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			posPrinter = ((MainActivity) Activity).POSPrinter;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void statusUpdateOccurred(final jpos.events.StatusUpdateEvent e)
		public override void statusUpdateOccurred(StatusUpdateEvent e)
		{
			Activity.runOnUiThread(() =>
			{

				deviceMessagesTextView.append("SUE: " + getSUEMessage(e.Status) + "\n");
				scroll();
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void outputCompleteOccurred(final jpos.events.OutputCompleteEvent e)
		public override void outputCompleteOccurred(OutputCompleteEvent e)
		{
			Activity.runOnUiThread(() =>
			{

				deviceMessagesTextView.append("OCE: " + e.OutputID + "\n");
				scroll();
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void errorOccurred(final jpos.events.ErrorEvent e)
		public override void errorOccurred(ErrorEvent e)
		{
			Activity.runOnUiThread(() =>
			{

				deviceMessagesTextView.append("Error: " + e + "\n");
				scroll();
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void directIOOccurred(final jpos.events.DirectIOEvent e)
		public override void directIOOccurred(DirectIOEvent e)
		{
			Activity.runOnUiThread(() =>
			{

				deviceMessagesTextView.append("DirectIO: " + e + "(" + getBatterStatusString(e.Data) + ")\n");

				if (e.Object != null)
				{
					deviceMessagesTextView.append(StringHelperClass.NewString((sbyte[]) e.Object) + "\n");
				}

				scroll();
			});
		}

		private void scroll()
		{
			Layout layout = deviceMessagesTextView.Layout;
			if (layout != null)
			{
				int y = layout.getLineTop(deviceMessagesTextView.LineCount) - deviceMessagesTextView.Height;
				if (y > 0)
				{
					deviceMessagesTextView.scrollTo(0, y);
					deviceMessagesTextView.invalidate();
				}
			}
		}

		private string getSUEMessage(int status)
		{
			switch (status)
			{
			case JposConst.JPOS_SUE_POWER_OFF_OFFLINE:
				return "Power off";

			case POSPrinterConst.PTR_SUE_COVER_OPEN:
				return "Cover Open";

			case POSPrinterConst.PTR_SUE_COVER_OK:
				return "Cover OK";

			case POSPrinterConst.PTR_SUE_REC_EMPTY:
				return "Receipt Paper Empty";

			case POSPrinterConst.PTR_SUE_REC_NEAREMPTY:
				return "Receipt Paper Near Empty";

			case POSPrinterConst.PTR_SUE_REC_PAPEROK:
				return "Receipt Paper OK";

			case POSPrinterConst.PTR_SUE_IDLE:
				return "Printer Idle";

				default:
					return "Unknown";
			}
		}

		private string getBatterStatusString(int status)
		{
			switch (status)
			{
			case 0x30:
				return "Full";

			case 0x31:
				return "High";

			case 0x32:
				return "Middle";

			case 0x33:
				return "Low";

				default:
					return "Unknwon";
			}
		}
	}

}