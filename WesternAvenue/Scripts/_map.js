(function () {
    'use strict';
     
    var app = angular.module('myapp', ['uiGmapgoogle-maps']); //dependency we should add to angular application  
      
    app.config(function (uiGmapGoogleMapApiProvider) {
        uiGmapGoogleMapApiProvider.configure({
            key: 'AIzaSyD-K8_caJKWfha8kMTjpGEq6hW3LnZx7FI',
            v: '3.17',
            libraries: 'weather,geometry,visualization'
        });
    })

    app.controller('mapsController', function ($scope, $http, $timeout)
    { 
        angular.element(document).ready(function () { 
            $scope.getData(); 
        });

         
        //this is default coordinates for the map when it loads for first time  
        //Location is close to Western Avenue Metra station
        $scope.map =
          {
              center:
              {
                  latitude: 41.902243, 
                  longitude: -87.703288
              },
              zoom: 14
          }
        $scope.markers = [];
        $scope.locations = [];
        $scope.getDatetime = new Date();

        //to get all the locations from the server  
        // Function to get the data
        $scope.getData = function ()
        {
            $http.get('/home/GetAllLocation').then(function (data) {

                $scope.getDatetime = new Date();

                $scope.locations = data.data;
                $scope.markers.length = 0;

                for (var i = 0; i < data.data.length; i++) {
                    //var myBounds = new $scope.map.LatLngBounds();

                    //var _tmp_position = new $scope.map.LatLng(
                    //  data.data[i].Lat,
                    // data.data[i].Long);
                    //myBounds.extend(_tmp_position);

                    $scope.markers.push
                    ({
                        id: data.data[i].LocationID,
                        coords:
                        {
                            latitude: data.data[i].Lat,
                            longitude: data.data[i].Long
                        },
                        title: data.data[i].title, //title of the makers  
                        address: data.data[i].Address, //Address of the makers  
                        image: data.data[i].ImagePath, //image --optional  
                        icon: data.data[i].ImagePath //image --optional  
                    });
                    //set map focus to center  
                    // $scope.map.center.latitude = data.data.Lat;
                    // $scope.map.center.longitude = data.data.Long;
                }


                //$scope.map.bounds = {
                //    northeast: {
                //        latitude: myBounds.getNorthEast().lat(),
                //        longitude: myBounds.getNorthEast().lng()
                //    },
                //    southwest: {
                //        latitude: myBounds.getSouthWest().lat(),
                //        longitude: myBounds.getSouthWest().lng()
                //    }
                //};

            },
            function () {
                console.log('Error');
            });
        }

        //service that gets makers info from server  
        $scope.ShowLocation = function (locationID) {
            $http.get('/home/GetMarkerData',
            {
                params:
                {
                    locationID: locationID
                }
            }).then(function (data) {
                $scope.markers = [];
                $scope.markers.push
                ({
                    id: data.data.LocationID,
                    coords:
                    {
                        latitude: data.data.Lat,
                        longitude: data.data.Long
                    },
                    title: data.data.title, //title of the makers  
                    address: data.data.Address, //Address of the makers  
                    image: data.data.ImagePath, //image --optional  
                    icon: data.data.ImagePath //image --optional  
                });
                //set map focus to center  
                $scope.map.center.latitude = data.data.Lat;
                $scope.map.center.longitude = data.data.Long;
            }, function () {
                alert('Error'); //shows error if connection lost or error occurs  
            });
        }
        //Show or Hide marker on map using options passes here  
        $scope.windowOptions =
          {
              show: true
          };
         
        // Function to replicate setInterval using $timeout service.
        $scope.intervalFunction = function ()
        {
            $timeout(function () {
                $scope.getData();
                $scope.intervalFunction();
            }, 30000)
        };

        // Kick off the interval
        $scope.intervalFunction();
    });
})();