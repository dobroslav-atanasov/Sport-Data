//$(document).ready(function () {
//    const map = new jsVectorMap({
//        selector: '#map',
//        map: 'world',
//        markers: [
//            { name: "Athens 1896, 2004", coords: [37.983810, 23.727539], style: { fill: 'red' } },
//            { name: "Paris 1900, 1924, 2024", coords: [48.864716, 2.349014], style: { fill: 'red' } },
//            { name: "St. Louis 1904", coords: [38.627003, -90.199402], style: { fill: 'red' } },
//            { name: "London 1908, 1948, 2012", coords: [51.509865, -0.118092], style: { fill: 'red' } },
//            { name: "Stockholm 1912", coords: [59.334591, 18.063240], style: { fill: 'red' } },
//            { name: "Antwerp 1920", coords: [51.260197, 4.402771], style: { fill: 'red' } },
//            { name: "Amsterdam 1928", coords: [52.377956, 4.897070], style: { fill: 'red' } },
//            { name: "Los Angeles 1932, 1984, 2028", coords: [34.052235, -118.243683], style: { fill: 'red' } },
//            { name: "Berlin 1936", coords: [52.520008, 13.404954], style: { fill: 'red' } },
//            { name: "Helsinki 1952", coords: [60.192059, 24.945831], style: { fill: 'red' } },
//            { name: "Melbourne 1956", coords: [-37.840935, 144.946457], style: { fill: 'red' } },
//            { name: "Rome 1960", coords: [41.902782, 12.496366], style: { fill: 'red' } },
//            { name: "Tokyo 1964, 2020", coords: [35.652832, 139.839478], style: { fill: 'red' } },
//            { name: "Mexico City 1968", coords: [19.432608, -99.133209], style: { fill: 'red' } },
//            { name: "Munich 1972", coords: [48.137154, 11.576124], style: { fill: 'red' } },
//            { name: "Montreal 1976", coords: [45.508888, -73.561668], style: { fill: 'red' } },
//            { name: "Moscow 1980", coords: [55.751244, 37.6184231], style: { fill: 'red' } },
//            { name: "Seoul 1988", coords: [37.532600, 127.024612], style: { fill: 'red' } },
//            { name: "Barcelona 1992", coords: [41.390205, 2.154007], style: { fill: 'red' } },
//            { name: "Atlanta 1996", coords: [33.753746, -84.386330], style: { fill: 'red' } },
//            { name: "Sydney 2000", coords: [-33.865143, 151.209900], style: { fill: 'red' } },
//            { name: "Beijing 2008", coords: [39.916668, 116.383331], style: { fill: 'red' } },
//            { name: "Rio de Janeiro 2016", coords: [-22.908333, -43.196388], style: { fill: 'red' } },
//            { name: "Brisbane 2032", coords: [-27.470125, 153.021072], style: { fill: 'red' } },
//        ],
//        labels: {
//            markers: {
//                render(marker, index) {
//                    return marker.name;
//                }
//            }
//        },
//        selectedRegions: ['GR', 'FR', 'US', 'GB', 'SE', 'BE', 'NL', 'DE', 'CN', 'FI', 'IT', 'AU', 'RU', 'CA', 'MX', 'JP', 'KR', 'ES', 'BR'],
//        //regionsSelectable: true
//        //lines: [{
//        //    from: 'Athens 1896, 2004',
//        //    to: 'Beijing 2008',
//        //    style: {
//        //        stroke: 'red',
//        //    }
//        //}],
//        //lineStyle: {
//        //    stroke: "#676767",
//        //    strokeWidth: 1.5,
//        //    fill: '#ff5566',
//        //    fillOpacity: 1,
//        //    strokeDasharray: '6 3 6', // OR: [6, 2, 6]
//        //    animation: true // Enables animation
//        //}
//        series: {
//            markers: [{
//                attribute: "fill",
//                legend: {
//                    title: "Olympic Games",
//                },
//                scale: {
//                    "Summer": "#FF0000",
//                    "Winter": "#c79efd",
//                },
//                //values: {
//                //    // Notice: the key must be a number of the marker.
//                //    0: "mScale1",
//                //    1: "mScale2",
//                //    2: "mScale2"
//                //}
//            }]
//        }
//    });
//});