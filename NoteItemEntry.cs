using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyApp
{
    public class NoteItemEntry : BaseItemEntry
    {
        private int _rn;
        private int _total;
        private Guid _id;
        private DateTime _created_date = DateTime.Now.ToUniversalTime();
        private DateTime _updated_date = DateTime.Now.ToUniversalTime();
        private JObject _param = new JObject();
        private string _title;
        private string _description;
        private bool _pin;
        private bool _fav;
        private int _deleted;

        [JsonProperty("rn")]
        public int Rn { get { return _rn; } set { SetProperty(ref _rn, value); } }
        [JsonProperty("total")]
        public int Total { get { return _total; } set { SetProperty(ref _total, value); } }
        [JsonProperty("id")]
        public Guid Id { get { return _id; } set { SetProperty(ref _id, value); } }
        [JsonProperty("created_date")]
        public DateTime CreatedDate { get { return _created_date; } set { SetProperty(ref _created_date, value); } }
        [JsonProperty("updated_date")]
        public DateTime UpdatedDate { get { return _updated_date; } set { SetProperty(ref _updated_date, value); } }
        [JsonProperty("param")]
        public JObject Param { get { return _param; } set { SetProperty(ref _param, value); } }
        [JsonProperty("title")]
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        [JsonProperty("description")]
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
        [JsonProperty("pin")]
        public bool Pin { get { return _pin; } set { SetProperty(ref _pin, value); } }
        [JsonProperty("fav")]
        public bool Fav { get { return _fav; } set { SetProperty(ref _fav, value); } }
        [JsonProperty("deleted")]
        public int Deleted { get { return _deleted; } set { SetProperty(ref _deleted, value); } }

        public NoteItemEntry() { }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public NoteItemEntry(JObject obj)
        {
            Rn = Convert.ToInt32(obj["rn"]);
            Total = Convert.ToInt32(obj["total"]);
            Id = Guid.Parse(obj["id_note"].ToString());
            CreatedDate = DateTime.Parse(obj["created_date"].ToString(), CultureInfo.CurrentCulture);
            UpdatedDate = DateTime.Parse(obj["updated_date"].ToString(), CultureInfo.CurrentCulture);
            Param = JObject.Parse(obj["param"].ToString());
            Title = obj["title"].ToString();
            Description = obj["description"].ToString();
            Pin = Convert.ToBoolean(obj["pin"]);
            Fav = Convert.ToBoolean(obj["fav"]);
            Deleted = Convert.ToInt32(obj["deleted"]);
        }
    }
}