using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMGExpression
{
   public class Expression : INotifyPropertyChanged
    {
        private double anger;
        private double contempt;
        private double disgust;
        private double engagement;
        private double fear;
        private double joy;
        private double sadness;
        private double surprise;
        private double valence;
        private string angerPercent;
        private string contemptPercent;
        private string disgustPercent;
        private string engagementPercent;
        private string fearPercent;
        private string joyPercent;
        private string sadnessPercent;
        private string surprisePercent;
        private string valencePercent;
        public double Anger {
            get { return anger; }
            set
            {
                if (value != anger)
                {
                    anger = value;
                    OnPropertyChanged("Anger");
                }
            }
        }
        public double Contempt
        {
            get { return contempt; }
            set
            {
                if (value != contempt)
                {
                    contempt = value;
                    OnPropertyChanged("Contempt");
                }
            }
        }
        public double Disgust
        {
            get { return disgust; }
            set
            {
                if (value != disgust)
                {
                    disgust = value;
                    OnPropertyChanged("Disgust");
                }
            }
        }
        public double Engagement
        {
            get { return engagement; }
            set
            {
                if (value != engagement)
                {
                    engagement = value;
                    OnPropertyChanged("Engagement");
                }
            }
        }
        public double Fear
        {
            get { return fear; }
            set
            {
                if (value != fear)
                {
                    fear = value;
                    OnPropertyChanged("Fear");
                }
            }
        }
        public double Joy
        {
            get { return joy; }
            set
            {
                if (value != joy)
                {
                    joy = value;
                    OnPropertyChanged("Joy");
                }
            }
        }
        public double Sadness
        {
            get { return sadness; }
            set
            {
                if (value != sadness)
                {
                    sadness = value;
                    OnPropertyChanged("Sadness");
                }
            }
        }
        public double Surprise
        {
            get { return surprise; }
            set
            {
                if (value != surprise)
                {
                    surprise = value;
                    OnPropertyChanged("Surprise");
                }
            }
        }
        public double Valence
        {
            get { return valence; }
            set
            {
                if (value != valence)
                {
                    valence = value;
                    OnPropertyChanged("Valence");
                }
            }
        }
        public string AngerPercent
        {
            get { return angerPercent; }
            set
            {
                if (value != angerPercent)
                {
                    angerPercent = value;
                    OnPropertyChanged("AngerPercent");
                }
            }
        }
        public string ContemptPercent
        {
            get { return contemptPercent; }
            set
            {
                if (value != contemptPercent)
                {
                    contemptPercent = value;
                    OnPropertyChanged("ContemptPercent");
                }
            }
        }
        public string DisgustPercent
        {
            get { return disgustPercent; }
            set
            {
                if (value != disgustPercent)
                {
                    disgustPercent = value;
                    OnPropertyChanged("DisgustPercent");
                }
            }
        }
        public string EngagementPercent
        {
            get { return engagementPercent; }
            set
            {
                if (value != engagementPercent)
                {
                    engagementPercent = value;
                    OnPropertyChanged("EngagementPercent");
                }
            }
        }
        public string FearPercent
        {
            get { return fearPercent; }
            set
            {
                if (value != fearPercent)
                {
                    fearPercent = value;
                    OnPropertyChanged("FearPercent");
                }
            }
        }
        public string JoyPercent
        {
            get { return joyPercent; }
            set
            {
                if (value != joyPercent)
                {
                    joyPercent = value;
                    OnPropertyChanged("JoyPercent");
                }
            }
        }
        public string SadnessPercent
        {
            get { return sadnessPercent; }
            set
            {
                if (value != sadnessPercent)
                {
                    sadnessPercent = value;
                    OnPropertyChanged("SadnessPercent");
                }
            }
        }
        public string SurprisePercent
        {
            get { return surprisePercent; }
            set
            {
                if (value != surprisePercent)
                {
                    surprisePercent = value;
                    OnPropertyChanged("SurprisePercent");
                }
            }
        }
        public string ValencePercent
        {
            get { return valencePercent; }
            set
            {
                if (value != valencePercent)
                {
                    valencePercent = value;
                    OnPropertyChanged("ValencePercent");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
