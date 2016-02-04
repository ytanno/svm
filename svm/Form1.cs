using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;


namespace svm
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			chart1.Series.Clear();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var points = new List<Point>();

			//File Dialog
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "(*.txt)|*.txt";
			ofd.FilterIndex = 2;
			ofd.Title = "Select 2D Data";
			ofd.RestoreDirectory = true;

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				var path = ofd.FileName;
				var lines = File.ReadAllLines(path);
				foreach (var line in lines)
				{
					var sp = line.Split(',');
					points.Add(new Point(int.Parse(sp[0]), int.Parse(sp[1])));
				}
				var rankList = svm(points);
				DrawGraph(points, rankList);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var rndPoints = GetRndPoint(10);
			var rankList = svm(rndPoints);
			DrawGraph(rndPoints, rankList);
		}

		private void DrawGraph(List<Point> rndPoints, List<Perpendicular> rank)
		{
			var higest = rank.OrderByDescending(x => x.PointDistAvg).FirstOrDefault();
			
			chart1.Series.Clear();
			chart1.Series.Add("Plot");
			chart1.Series["Plot"].IsVisibleInLegend = false;
			chart1.Series["Plot"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;

			chart1.Series.Add("Line");
			chart1.Series["Line"].IsVisibleInLegend = false;
			chart1.Series["Line"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

			foreach (var p in rndPoints)
			{
				chart1.Series["Plot"].Points.AddXY(p.X, p.Y);
			}

			var xMax = rndPoints.Max(p => p.X);
			for( var i = 0; i < xMax; i++)
			{
				var y = higest.A * i + higest.B;
				if ( y < 150 && y > -50) chart1.Series["Line"].Points.AddXY(i, y);
			}


			
			//draw all Perpendicular
			/*
			for(int i = 0; i < rank.Count; i++)
			{
				var str = "Line" + i;
				chart1.Series.Add(str);
				chart1.Series[str].IsVisibleInLegend = false;
				chart1.Series[str].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

				for (var x = 0; x < xMax; x++)
				{
					var y = rank[i].A * x + rank[i].B;
					if ( y < 150 && y > -50) chart1.Series[str].Points.AddXY(x, y);
				}
			}
			*/
			
		}



		private List<Perpendicular> svm(List<Point> points)
		{
			var rankingList = new List<Perpendicular>();

			for (int i = 0; i < points.Count; i++)
			{
				for (int j = i + 1; j < points.Count; j++)
				{
					//define Perpendicular
					var perpendicular = new Perpendicular(points[i], points[j]);
					if (double.IsNaN(perpendicular.A) || double.IsNaN(perpendicular.B)) continue;
					if (double.IsInfinity(perpendicular.A) || double.IsInfinity(perpendicular.B)) continue;

					var distanceList = new List<double>();

					foreach(var p in points)
					{
						//cal length point and perpendicular 
						var pLen = Math.Sqrt(Math.Pow(p.X - 0, 2) + Math.Pow(p.Y - perpendicular.B, 2));

						var aVecX = 1.0;
						var aVecY = perpendicular.A;

						var cosThita = (p.X * aVecX + (p.Y - perpendicular.B) * aVecY) /
										(Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y - perpendicular.B, 2)) 
										* Math.Sqrt(Math.Pow(aVecX, 2) + Math.Pow(aVecY, 2)));

						var thita = Math.Acos(cosThita);
						var dist = pLen * Math.Sin(thita);
						distanceList.Add(dist);
					}
					perpendicular.PointDistAvg = distanceList.Average();
					perpendicular.Output();
					rankingList.Add(perpendicular);
				}
			}
			return rankingList;
		}


		private List<Point> GetRndPoint(int num)
		{
			var list = new List<Point>();
			var rnd = new System.Random();
			for (var i = 0; i < num; i++)
				list.Add(new Point((int)rnd.Next(100), (int)rnd.Next(100)));
			return list;
		}

		
	}

	public class Perpendicular
	{
		public double A, B; // y= Ax + B;

		//Average point dist 
		public double PointDistAvg = 0.0;

		public Point UsedP1, UsedP2;

		public Perpendicular(Point p1, Point p2)
		{
			var a = (double)(p1.Y - p2.Y) / (double)(p1.X - p2.X);
			A = -1.0 / a;
			var mx = (double)(p1.X + p2.X) / 2.0;
			var my = (double)(p1.Y + p2.Y) / 2.0;
			B = my - mx * A;
			UsedP1 = p1;
			UsedP2 = p2;
		}

		public void Output()
		{
			//using (var sw = new StreamWriter(Environment.CurrentDirectory + @"\test.txt", true))
			//{
				Console.WriteLine("y={0:f3}x + {1:f3} avg dist {2:f3}", A, B, PointDistAvg);
				//sw.WriteLine("y={0:f3}x + {1:f3} avg dist {2:f3}", A, B, PointDistAvg);
			//}
		}


	}

}
