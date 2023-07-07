using Android.Locations;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using System.Linq;
using GpsApp;
using Android.Widget;
using Java.Util;
using Java.Text;
using Android.Graphics;

/// <summary>
/// A RecyclerView.Adapter for displaying location information in a list.
/// 
/// I considered doing this as a TableLayout, so it'd be easier to have consistent column widths.  However,
/// since the data is not freeform (and is numeric, so localization is less likely to expand the text), we can
/// stick with RecyclerView to conserve memory.
/// </summary>
public class LocationRecyclerAdapter : RecyclerView.Adapter
{
    private List<Location> _data;
    private readonly DateFormat _dateFormatter;
    private readonly DateFormat _timeFormatter;
    private readonly DecimalFormat _singleDecimalFormatter;

    // conversion factor from meters per second to miles per hour.
    const double SpeedConversion = 2.23694;

    public LocationRecyclerAdapter()
    {
        _data = new List<Location>();
        _dateFormatter = DateFormat.GetDateInstance(DateFormat.Short);
        _timeFormatter = DateFormat.GetTimeInstance(DateFormat.Short);
        _singleDecimalFormatter = new DecimalFormat("#.#");
    }

    public override int ItemCount => _data.Count;

    public override long GetItemId(int position)
    {
        // In a real app, we'd return a stable ID here and get a nice animation when the dataset changes
        return position;
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var inflater = LayoutInflater.From(parent.Context);
        var row = inflater.Inflate(Resource.Layout.item_location, parent, false);
        return new LocationViewHolder(row);
    }
    public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
    {
        var location = _data[position];
        var date = new Date(location.Time);

        var holder = (LocationViewHolder) viewHolder;

        holder.DateTextView.Text = _dateFormatter.Format(date);
        holder.TimeTextView.Text = _timeFormatter.Format(date);
        holder.LatTextView.Text = _singleDecimalFormatter.Format(location.Latitude);
        holder.LongTextView.Text = _singleDecimalFormatter.Format(location.Longitude);
        holder.BearingTextView.Text = _singleDecimalFormatter.Format(location.Bearing);
        holder.SpeedTextView.Text = (location.Speed * SpeedConversion).ToString();
        holder.ProviderNameTextView.Text = location.Provider;

        // TODO: put in colors.xml
        holder.ItemView.SetBackgroundColor(position % 2 == 0 ? Color.PaleTurquoise : Color.White);
    }

    public void SetData(List<Location> newData)
    {
        // always show most-recent first
        _data = newData.OrderByDescending(l => l.Time).ToList();
        NotifyDataSetChanged();
    }

    protected class LocationViewHolder : RecyclerView.ViewHolder
    {
        public TextView DateTextView;
        public TextView TimeTextView;
        public TextView LatTextView;
        public TextView LongTextView;
        public TextView BearingTextView;
        public TextView SpeedTextView;
        public TextView ProviderNameTextView;

        public LocationViewHolder(View item) : base(item)
        {
            DateTextView = item.FindViewById<TextView>(Resource.Id.location_date_text);
            TimeTextView = item.FindViewById<TextView>(Resource.Id.location_time_text);
            LatTextView = item.FindViewById<TextView>(Resource.Id.location_lat_text);
            LongTextView = item.FindViewById<TextView>(Resource.Id.location_long_text);
            BearingTextView = item.FindViewById<TextView>(Resource.Id.location_bearing_text);
            SpeedTextView = item.FindViewById<TextView>(Resource.Id.location_speed_text);
            ProviderNameTextView = item.FindViewById<TextView>(Resource.Id.location_provider_text);
        }
    }
}
