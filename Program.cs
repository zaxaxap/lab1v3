using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;

namespace Lab1v3
{
    struct DataItem
    {
        public double x { get; set; }
        public double y { get; set; }
        public Vector2 component { get; set; }
        public DataItem(double x, double y, System.Numerics.Vector2 component){
            this.x = x;
            this.y = y;
            this.component = component;
            }
        public string ToLongString(string format)
        {
            return ($"X = {x.ToString(format)}, Y = {y.ToString(format)}, Field X component = {component.X.ToString(format)}, Field Y component = {component.Y.ToString(format)}, Module of field = { Math.Sqrt(this.component.X * this.component.X + this.component.Y * this.component.Y).ToString(format)}\n");
        }
        public override string ToString()
        {
            return String.Format("Точка измерения: {0:f2}, {1:f2}\n Компоненты вектора поля и его модуль: {2:f2}, {3:f2}; {4:f3}",  this.x, this.y, this.component.X, this.component.Y, Math.Sqrt(this.component.X * this.component.X + this.component.Y * this.component.Y));
        }
    }
    delegate Vector2 FdblVector2(double x, double y); 

    abstract class V3Data
    {
        public string id { get; }
        public DateTime date { get; }
        public V3Data(string id, DateTime date)
        {
            this.id = id;
            this.date = date;
        }
        public abstract int Count { get; }
        public abstract double MaxDistance { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return String.Format("Идентификатор: {0} \nДата измерения: {1}\n", id, date);
        }
    }


    class V3DataList: V3Data
    {
        public List<DataItem> data { get; }
        public V3DataList(string id, DateTime date) : base(id, date) 
        {
            data = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
           for (int i = 0; i < data.Count; i++)
            {
                if((data[i].x == newItem.x) && (data[i].y == newItem.y))
                {
                    return false;
                }
            }
            data.Add(newItem);
            return true;
        }
        public int AddDefaults(int nItems, FdblVector2 F)
        {
            int k = (int)Math.Floor(Math.Sqrt(nItems));
            int CountOfNew = 0;
            for(int i = 0; i < k; i++)
            {
                for(int j = 0; j < k; j++)
                {
                    if(this.Add(new DataItem(j, i, F(j, i))))
                    {
                        CountOfNew++;
                    }
                    
                }
            }
            for(int i = 0; i < nItems - k*k; i++)
            {
                if (this.Add(new DataItem(k, i, F(k, i))))
                {
                    CountOfNew++;
                }
            }
            return CountOfNew;
        }
        public override int Count
        {
            get { return data.Count; }
        }
        public override double MaxDistance
        {
            get
            {
                double max = 0;
                double distance;
                for (int i = 0; i < (data.Count - 1); i++)
                {
                    for(int j = i + 1; j < data.Count; j++)
                    {
                        distance = Math.Sqrt((data[i].x - data[j].x) * (data[i].x - data[j].x) + (data[i].y - data[j].y) * (data[i].y - data[j].y));
                        if (distance > max)
                        {
                            max = distance;
                        }
                    }
                }
                return max;
            }
        }
        public override string ToString()
        {
            return "V3DataList\n" + base.ToString() + "Количество элементов: " + data.Count + "\n";
        }
        public override string ToLongString(string format)
        {
            StringBuilder str =  new StringBuilder(ToString());
            for(int i = 0; i < data.Count; i++)
            {
                str.Append(data[i].ToLongString(format));
            }
            return str.ToString();

        }
    }


    class V3DataArray: V3Data
    {
        Vector2[,] dimensions { get; }
        public double stepX { get; }
        public double stepY { get; }
        public int nodesX { get; }
        public int nodesY { get; }
        public V3DataArray(string id, DateTime date): base(id, date)
        {
            dimensions = new Vector2[0,0];
        }
        public V3DataArray(string id, DateTime date, int nodesX, int nodesY, double stepX, double stepY, FdblVector2 F): base(id, date)
        {
            dimensions = new Vector2[nodesY, nodesX];
            this.stepX = stepX;
            this.nodesX = nodesX;
            this.nodesY = nodesY;
            this.stepY = stepY;
            for(int i = 0; i < nodesY; i++)
            {
                for(int j = 0; j < nodesX; j++)
                {
                    dimensions[i, j] = F(i * stepY, j * stepX);
                }
            }
        }
        public override int Count 
        {
            get
            {
                return nodesX * nodesY;
            }
        }
        public override double MaxDistance 
        {
            get
            {
                return Math.Sqrt((nodesX - 1) * stepX * (nodesX - 1) * stepX + (nodesY - 1) * stepY * (nodesY - 1) * stepY);
            }
        }
        public override string ToString()
        {
            return "V3DataArray\n" + base.ToString() + "Размер шага по X и по Y:" + stepX + " " + stepY + "\n" +"Размер сетки: " + nodesX + "*" + nodesY + "\n" ;
        }
        public override string ToLongString(string format)
        {
            StringBuilder str = new StringBuilder(ToString());
            for(int i = 0; i < nodesY; i++)
            {
                for(int j = 0; j < nodesX; j++)
                {
                    str.Append($"X = {(j * stepX).ToString(format)}, Y = {(i * stepY).ToString(format)}, Field X component = {dimensions[i,j].X.ToString(format)}, Field Y component = {dimensions[i,j].X.ToString(format)}, Module of field = { Math.Sqrt(dimensions[i,j].X * dimensions[i, j].X + dimensions[i, j].Y * dimensions[i, j].Y).ToString(format)}\n");
                }
            }

            return str.ToString();
        }
        public static explicit operator V3DataList(V3DataArray param)
        {
            V3DataList List = new V3DataList(param.id, param.date);
            for (int i = 0; i < param.nodesY; i++)
            {
                for (int j = 0; j < param.nodesX; j++)
                {
                    List.Add(new DataItem(j * param.stepX, i * param.stepY, param.dimensions[i,j]));
                }
            }
            return List;
        }
    }
    


    class V3MainCollection
    {
        private List<V3Data> List = new List<V3Data>();
        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        public V3Data this[int index]
        {
            get => List[index];
        }
        public bool Contains(string ID)
        {
            for (int i = 0; i < List.Count; i++)
            {
               if(List[i].id == ID)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Add(V3Data v3Data)
        {
            for(int i = 0; i < List.Count; i++)
            {
                if(v3Data.id == List[i].id)
                {
                    return false;
                }
            }
            List.Add(v3Data);
            return true;
        }
        public string ToLongString(string format)
        {
            StringBuilder str = new StringBuilder();
            for(int i = 0; i < List.Count; i++)
            {
                str.Append(List[i].ToLongString(format));
            }
            return str.ToString();
        }
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < List.Count; i++)
            {
                str.Append(List[i].ToString());
            }
            return str.ToString();
        }
    }

    static class DefaultType
    {
        public static Vector2 AsCoordinat(double x, double y)
        {
            return new Vector2((float)x, (float)y);
        }
        public static Vector2 normalXplate(double x, double y)
        {
            return new Vector2(0, 1);
        } 
        public static Vector2 normalPoint(double x, double y)
        {
            return new Vector2((float)(Math.Cos(x / Math.Sqrt(x * x + y * y)) / (x*x + y*y)), (float)(Math.Cos(y / Math.Sqrt(x * x + y * y)) / (x * x + y * y)));
        }
    }
   class Program
    {
        static void Main(string[] args)
        {
            //1.
            FdblVector2 coord = DefaultType.AsCoordinat;
            V3DataArray myArray = new V3DataArray("First", new DateTime(2021, 10, 12), 5, 5, 2, 2, coord);
            Console.WriteLine(myArray.ToLongString("f"));
            V3DataList myList = (V3DataList)myArray;
            Console.WriteLine(myList.ToLongString("f"));
            Console.WriteLine("Array count and max distance: " + myArray.Count + " " + myArray.MaxDistance + "\n");
            Console.WriteLine("List count and max distance: " + myList.Count + " " + myList.MaxDistance + "\n");


            //2.
            V3MainCollection myCollection = new V3MainCollection();
            coord = DefaultType.normalXplate;
            V3DataArray myArray2 = new V3DataArray("Second", new DateTime(2021, 10, 12), 5, 5, 2, 2, coord);
            V3DataArray myArray3 = new V3DataArray("Third", new DateTime(2021, 10, 12), 4, 4, 3, 3, coord);
            V3DataList myList2 = (V3DataList)myArray3;

            coord = DefaultType.normalPoint;
            V3DataArray myArray4 = new V3DataArray("Forth", new DateTime(2021, 10, 12), 4, 4, 0.5, 0.5, coord);
            V3DataList myList3 = (V3DataList)myArray4;

            myCollection.Add(myArray);
            myCollection.Add(myArray2);
            myCollection.Add(myList2);
            myCollection.Add(myList3);
            Console.WriteLine(myCollection.ToLongString("f"));

            //3.
            for(int i = 0; i < myCollection.Count; i++)
            {
                Console.WriteLine("Collection "+ i +" element count and max distance: " + myCollection[i].Count + " " + myCollection[i].MaxDistance + "\n");
            }
        }
    }
}
