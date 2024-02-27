using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Providers;
using BruTile.Wms;
using System.Threading;
using Mapsui.Animations;

namespace TranSys
{
    public static class LayersCreator
    {
        public static Mapsui.Layers.ILayer getLineLayer(List<MPoint> points, string color, int width)
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            foreach (MPoint point in points)
            {
                var spherical = SphericalMercator.FromLonLat(point);
                coordinates.Add(new Coordinate(spherical.X, spherical.Y));
            }

            var lineString = new LineString(coordinates.ToArray());

            return new Mapsui.Layers.MemoryLayer
            {
                Features = new[] { new GeometryFeature { Geometry = lineString } },
                Name = "LineLayer",
                IsMapInfoLayer = true,
                Style = new VectorStyle
                {
                    Fill = null,
                    Outline = null,
                    Line = { Color = Mapsui.Styles.Color.FromString(color), Width = width }
                }
            };
        }

        public static Mapsui.Layers.Layer getStationsLayer(List<Station> stations)
        {
            List<Mapsui.IFeature> features = new List<Mapsui.IFeature>();
            foreach (var station in stations)
            {
                var point = SphericalMercator.FromLonLat(station._long, station._lat);
                var feature = new GeometryFeature();
                feature.Geometry = new NetTopologySuite.Geometries.Point(point.x, point.y);
                feature["Name"] = station._name;
                features.Add(feature);
            }

            return new Mapsui.Layers.Layer
            {
                DataSource = new MemoryProvider(features),
                Name = "StationsLayer",
                IsMapInfoLayer = true,
                Style = CreateStationStyle()
            };

            SymbolStyle CreateStationStyle()
            {
                var bitmapID = typeof(MainWindow).LoadBitmapId("bus-stop.png");

                return new SymbolStyle { BitmapId = bitmapID, SymbolScale = 0.09, SymbolOffset = new Offset(0, 200) };
            }
        }
        
        public static Mapsui.Layers.Layer getMidpointsLayer(List<Midpoint> midpoints)
        {
            List<IFeature> features = new List<IFeature>();
            foreach(var midpoint in midpoints)
            {
                var point = midpoint.point;
                var feature = new GeometryFeature();
                feature.Geometry = new NetTopologySuite.Geometries.Point(point.X, point.Y);
                feature["begin"] = midpoint.begin;
                feature["end"] = midpoint.end;
                features.Add(feature);
            }
            return new Mapsui.Layers.Layer
            {
                DataSource = new MemoryProvider(features),
                Name = "MidpointsLayer",
                IsMapInfoLayer = true,
                Style = CreateMidpointStyle()
            };

            SymbolStyle CreateMidpointStyle()
            {
                var bitmapID = typeof(MainWindow).LoadBitmapId("circle.png");

                return new SymbolStyle { BitmapId = bitmapID, SymbolScale = 0.05 };
            }
        }
    }
}
