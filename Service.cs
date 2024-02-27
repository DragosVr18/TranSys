using BruTile;
using Itinero;
using Itinero.Osm;
using Itinero.IO.Osm;
using Neo4j.Driver;
using OsmSharp.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Itinero.Profiles;
using Mapsui.Projections;
using System.ComponentModel.DataAnnotations;
using Mapsui;
using System.Threading;
using System.Windows;
using BruTile.Wms;
using BruTile.Wmts.Generated;

namespace TranSys
{
    public class Service: IDisposable
    {
        private readonly IDriver _driver;
        private RouterDb _routerDB;
        private List<Station> _stations;
        private List<Line> _lines;
        public RouterDb RouterDb
        {
            get { return _routerDB; }
        }
        public List<Station> Stations
        {
            get { return _stations; }
        }
        public List<Line> Lines
        {
            get { return _lines; }
        }

        public Service(string uri, string username, string password)
        {
            /*var bus = DynamicVehicle.LoadFromStream(File.OpenRead("RouterData/bus.lua"));
            _routerDB = new RouterDb();
            using (var stream = new FileInfo("RouterData/romania.pbf").OpenRead())
            {
                _routerDB.LoadOsmData(stream, bus);
            }
            using (var stream = new FileInfo("RouterData/romania.routerdb").Open(FileMode.Create))
            {
                _routerDB.Serialize(stream);
            }*/
            using (var stream = new FileInfo("RouterData/romania.routerdb").OpenRead())
            {
                _routerDB = RouterDb.Deserialize(stream);
            }
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
            resetStations();
            resetLines();
        }
        public void Dispose()
        {
            _driver?.Dispose();
        }

        public List<double> getCenterLocation()
        {
            var session = _driver.Session();

            return session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (l:CenterLocation) RETURN [l.long,l.lat,l.res]");
                    return result.Select(record => record[0].As<List<double>>()).First();
                });
        }

        public void addStation(string name, List<string> vehicleTypes, int capacity, double _long, double lat)
        {
            var session = _driver.Session();

            session.ExecuteWrite(
                tx => tx.Run("CREATE (s:Station {name: $name, vehicleTypes: $vehicleTypes, capacity: $capacity, long: $_long, lat: $lat})",
                new { name, vehicleTypes, capacity, _long, lat }).Consume()
                );

            resetStations();
        }

        public void resetStations()
        {
            var session = _driver.Session();

            _stations = session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (s:Station) RETURN s.name, s.vehicleTypes, s.capacity, s.long, s.lat");
                    return result.Select(record => new Station(
                        record[0].As<string>(),
                        record[1].As<List<string>>(),
                        record[2].As<int>(),
                        record[3].As<double>(),
                        record[4].As<double>())).ToList();
                });
        }

        public Station? findStationByName(string name)
        {
            var session = _driver.Session();

            return session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (s:Station { name: $name }) RETURN s.name, s.vehicleTypes, s.capacity, s.long, s.lat",
                        new { name });
                    return result.Select(record => new Station(
                        record[0].As<string>(),
                        record[1].As<List<string>>(),
                        record[2].As<int>(),
                        record[3].As<double>(),
                        record[4].As<double>())).First();
                });
        }

        public Line? findLineByName(string name)
        {
            var session = _driver.Session();

            return session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (s:Line { name: $name }) RETURN s.name, s.vehicleType, s.description, s.begin, s.back",
                        new { name });
                    return result.Select(record => new Line(
                        record[0].As<string>(),
                        record[1].As<string>(),
                        record[2].As<string>(),
                        record[3].As<string>(),
                        record[4].As<string>())).First();
                });
        }

        public void resetLines()
        {
            var session = _driver.Session();

            _lines =  session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (s:Line) RETURN s.name, s.vehicleType, s.description, s.begin, s.back");
                    return result.Select(record => new Line(
                        record[0].As<string>(),
                        record[1].As<string>(),
                        record[2].As<string>(),
                        record[3].As<string>(),
                        record[4].As<string>())).ToList();
                });
        }

        public List<Line> getLinesByType(string vehicleType)
        {
            var session = _driver.Session();

            return session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (s:Line {vehicleType:'"+vehicleType+"'}) RETURN s.name, s.vehicleType, s.description, s.begin, s.back ORDER BY s.name");
                    return result.Select(record => new Line(
                        record[0].As<string>(),
                        record[1].As<string>(),
                        record[2].As<string>(),
                        record[3].As<string>(),
                        record[4].As<string>())).ToList();
                });
        }

        public async Task<List<MPoint>> getLinePoints(string name)
        {
            var pointsSession = _driver.AsyncSession();

            var pointsQueryString = "MATCH (s:Station)-[:LINE_"+name+"]-(t:Station) RETURN DISTINCT s.long, s.lat";
            List<MPoint> points = await pointsSession.ExecuteReadAsync(
                async tx =>
                {
                    var pts = new List<MPoint>();
                    var result = await tx.RunAsync(pointsQueryString);
                    while(await result.FetchAsync())
                    {
                        pts.Add(new MPoint(result.Current[0].As<double>(), result.Current[1].As<double>()));
                    }
                    return pts;
                });

            var midpointsSession = _driver.AsyncSession();

            var midpointsQueryString = "MATCH (s:Station)-[l:LINE_" + name + "]-(t:Station) WHERE l.Midpoint IS NOT NULL RETURN DISTINCT s.name, t.name, l.Midpoint[0], l.Midpoint[1], l.Midpoint[2], l.Midpoint[3]";
            List<Midpoint> midpoints = await midpointsSession.ExecuteReadAsync(
                async tx =>
                {
                    var mdpts = new List<Midpoint>();
                    var result = await tx.RunAsync(midpointsQueryString);
                    while (await result.FetchAsync())
                    {
                        mdpts.Add(new Midpoint
                        {
                            begin = findStationByName(result.Current[0].As<string>()),
                            end = findStationByName(result.Current[1].As<string>()),
                            point = new Mapsui.MPoint(
                            double.Parse(string.Format("{0},{1}", result.Current[4], result.Current[5])),
                            double.Parse(string.Format("{0},{1}", result.Current[2], result.Current[3])))
                        });
                    }
                    return mdpts;
                });

            for (int i = 0; i < midpoints.Count - 1; i+=2)
            {
                int index = points.FindIndex(x => x.X == midpoints[i].begin._long && x.Y == midpoints[i].begin._lat);
                points.Insert(index + 1, midpoints[i].point);
            }
            return points;
        }

        public List<MPoint> getRoute(string name)
        {
            List<MPoint> points = new List<MPoint>();
            List<MPoint> stationPoints = new List<MPoint>();

            //RouterDb routerDB = MainWindow._service.RouterDb;
            var router = new Router(_routerDB);
            var profile = DynamicVehicle.LoadFromStream(File.OpenRead("RouterData/bus.lua")).Shortest();

            stationPoints = Task.Run(() => getLinePoints(name)).Result;
            int l = stationPoints.Count;
            for (int i = 0; i < stationPoints.Count; i++)
            {
                var start = router.Resolve(profile, (float)stationPoints[i].Y, (float)stationPoints[i].X);
                var end = router.Resolve(profile, (float)stationPoints[(i + 1) % l].Y, (float)stationPoints[(i + 1) % l].X);
                var path = router.Calculate(profile, start, end);
                foreach (var p in path.Shape)
                {
                    points.Add(new MPoint(p.Longitude, p.Latitude));
                }
            }
            return points;
        }

        public void addLine(string name, string vehicleType, string description, string begin, string back, List<Station> stations, List<Midpoint> midpoints)
        {
            var session = _driver.Session();

            session.ExecuteWrite(
                tx => tx.Run("CREATE (l:Line {name: $name, vehicleType: $vehicleType, description: $description, begin: $begin, back: $back})",
                new { name, vehicleType, description, begin, back }).Consume()
                );

            for (int i = 0; i < stations.Count; i++)
            {
                string query = "Match (s:Station {name:'" + stations[i]._name + "'}), (t:Station {name:'" + stations[(i + 1) % stations.Count]._name + "'}) create (s)-[:LINE_" + name;
                var pts = midpoints.FindAll(x => x.begin._name == stations[i]._name && x.end._name == stations[(i + 1) % stations.Count]._name);
                if (pts.Count > 0)
                {
                    var sp = pts.First();
                    query+= " {Midpoint:";
                    var p = SphericalMercator.ToLonLat(sp.point);
                    query += "[" + p.Y + "," + p.X + "]}";
                }
                query += "]->(t)";
                session.ExecuteWrite( tx => tx.Run(query).Consume());
            }
            resetLines();
        }

        public List<Vehicle> getAllVehicles()
        {
            var session = _driver.Session();

            return session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (v:Vehicle) RETURN v.id, v.model, v.type, v.assignedLine, v.tourDepartures, v.backDepartures ORDER BY v.id");
                    return result.Select(record => new Vehicle(
                        record[0].As<int>(),
                        record[1].As<string>(),
                        record[2].As<string>(),
                        record[3].As<string>(),
                        record[4].As<List<OffsetTime>>(),
                        record[5].As<List<OffsetTime>>())).ToList();
                });
        }

        public List<Vehicle> GetVehiclesAssignedToLine(string lineName)
        {
            var session = _driver.Session();

            return session.ExecuteRead(
                tx =>
                {
                    var result = tx.Run("MATCH (v:Vehicle {assignedLine:'"+lineName+"'}) RETURN v.id, v.model, v.type, v.assignedLine, v.tourDepartures, v.backDepartures ORDER BY v.id");
                    return result.Select(record => new Vehicle(
                        record[0].As<int>(),
                        record[1].As<string>(),
                        record[2].As<string>(),
                        record[3].As<string>(),
                        record[4].As<List<OffsetTime>>(),
                        record[5].As<List<OffsetTime>>())).ToList();
                });
        }

        public void addVehicle(int id, string model, string type, string assignedLine, List<OffsetTime> tourDepartures, List<OffsetTime> backDepartures)
        {
            var session = _driver.Session();

            string query = "CREATE (v:Vehicle{id:" + id + ",model: '" + model + "',type:'" + type + "',assignedLine:'" + assignedLine + "',tourDepartures:[";
            foreach(var time in tourDepartures)
            {
                query += "time('" + time.Hour + ":" + time.Minute + "'),";
            }
            query = query.Remove(query.Length - 1, 1);
            query += "],backDepartures:[";
            foreach (var time in backDepartures)
            {
                query += "time('" + time.Hour + ":" + time.Minute + "'),";
            }
            query = query.Remove(query.Length - 1, 1);
            query += "]})";

            session.ExecuteWrite(
                tx => tx.Run(query).Consume());
        }

        public int getFirstAvailableID()
        {
            var vehicles = getAllVehicles();
            int i = 0;
            while (vehicles.Find(x => x.Id == i) != null)
                i++;
            return i;
        }

        public void updateStationName(string stationName, string newName)
        {
            var session = _driver.Session();

            var query = "MATCH (s:Station) WHERE s.name='" + stationName + "' SET s.name='" + newName + "'";

            session.ExecuteWrite(
                tx => tx.Run(query).Consume());
            resetStations();
        }

        public void updateStationCoordinates(string stationName, double newLong, double newLat)
        {
            var session = _driver.Session();

            session.ExecuteWrite(
            tx => tx.Run("MATCH(s: Station) WHERE s.name = $stationName SET s.lat = $newLat, s.long = $newLong",
            new {stationName, newLat, newLong }).Consume());
            resetStations();
        }

        public void updateStationCapacity(string stationName, double newCapacity)
        {
            var session = _driver.Session();

            var query = "MATCH (s:Station) WHERE s.name='" + stationName + "' SET s.capacity=" + newCapacity + "";

            session.ExecuteWrite(
                tx => tx.Run(query).Consume());
            resetStations();
        }
    }
}
