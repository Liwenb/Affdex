using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visifire.Charts;

namespace SMGExpression.Charts
{
    public class DataPoints : INotifyPropertyChanged
    {
        private DataPointCollection boyPoint;
        private DataPointCollection girlPoint;
        private DataPointCollection avgPoint;
        private DataPointCollection primaryPoint;
        private DataPointCollection age;
        private DataPointCollection income;
        private DataPointCollection address;
        private DataPointCollection job;
        private DataPointCollection education;
        private DataPointCollection agegroup;
        private DataPointCollection timesplit;
        private DataPointCollection grouptimesplit;
        private DataPointCollection yuliu1;
        private DataPointCollection yuliu2;
        private DataPointCollection yuliu3;
        private DataPointCollection yuliu4;
        private DataPointCollection yuliu5;
        public DataPoints()
        {
            yuliu1 = new DataPointCollection();
            yuliu2 = new DataPointCollection();
            yuliu3 = new DataPointCollection();
            yuliu4 = new DataPointCollection();
            yuliu5 = new DataPointCollection();
            grouptimesplit = new DataPointCollection();
            timesplit = new DataPointCollection();
            boyPoint = new DataPointCollection();
            girlPoint = new DataPointCollection();
            avgPoint = new DataPointCollection();
            primaryPoint = new DataPointCollection();
            age = new DataPointCollection();
            income = new DataPointCollection();
            address = new DataPointCollection();
            job = new DataPointCollection();
            education = new DataPointCollection();
            agegroup = new DataPointCollection();
        }
        public DataPointCollection Yuliu2
        {
            get { return yuliu2; }
            set
            {
                if (yuliu2 != value)
                {
                    yuliu2 = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Yuliu2"));
                }
            }
        }
        public DataPointCollection Yuliu5
        {
            get { return yuliu5; }
            set
            {
                if (yuliu5 != value)
                {
                    yuliu5 = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Yuliu5"));
                }
            }
        }
        public DataPointCollection Yuliu4
        {
            get { return yuliu4; }
            set
            {
                if (yuliu4 != value)
                {
                    yuliu4 = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Yuliu4"));
                }
            }
        }
        public DataPointCollection Yuliu3
        {
            get { return yuliu3; }
            set
            {
                if (yuliu3 != value)
                {
                    yuliu3 = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Yuliu3"));
                }
            }
        }
        public DataPointCollection Yuliu1
        {
            get { return yuliu1; }
            set
            {
                if (yuliu1 != value)
                {
                    yuliu1 = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Yuliu1"));
                }
            }
        }
        public DataPointCollection Grouptimesplit
        {
            get { return grouptimesplit; }
            set
            {
                if (grouptimesplit != value)
                {
                    grouptimesplit = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Grouptimesplit"));
                }
            }
        }
        public DataPointCollection Timesplit
        {
            get { return timesplit; }
            set
            {
                if (timesplit != value)
                {
                    timesplit = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Timesplit"));
                }
            }
        }
        public DataPointCollection User_group
        {
            get { return agegroup; }
            set
            {
                if (agegroup != value)
                {
                    agegroup = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Agegroup"));
                }
            }
        }
        public DataPointCollection User_edu
        {
            get { return education; }
            set
            {
                if (education != value)
                {
                    education = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Education"));
                }
            }
        }
        public DataPointCollection User_job
        {
            get { return job; }
            set
            {
                if (job != value)
                {
                    job = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Job"));
                }
            }
        }
        public DataPointCollection Address
        {
            get { return address; }
            set
            {
                if (address != value)
                {
                    address = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Address"));
                }
            }
        }
        public DataPointCollection Income
        {
            get { return income; }
            set
            {
                if (income != value)
                {
                    income = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Income"));
                }
            }
        }
        public DataPointCollection User_Age
        {
            get { return age; }
            set
            {
                if (age != value)
                {
                    age = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public DataPointCollection PrimaryPoint
        {
            get { return primaryPoint; }
            set
            {
                if (primaryPoint != value)
                {
                    primaryPoint = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("PrimaryPoint"));
                }
            }
        }
        public DataPointCollection BoyPoint
        {
            get { return boyPoint; }
            set
            {
                if (boyPoint != value)
                {
                    boyPoint = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("BoyPoint"));
                }
            }
        }
        public DataPointCollection GirlPoint
        {
            get { return girlPoint; }
            set
            {
                if (girlPoint != value)
                {
                    girlPoint = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("GirlPoint"));
                }
            }
        }
        public DataPointCollection AvgPoint
        {
            get { return avgPoint; }
            set
            {
                if (avgPoint != value)
                {
                    avgPoint = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("AvgPoint"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
