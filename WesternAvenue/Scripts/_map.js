(function () {
    'use strict';

    var app = angular.module('myapp', ['uiGmapgoogle-maps']); //dependency we should add to angular application  
      
    app.controller('mapsController', function ($scope, $http) {

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

         
        //to get all the locations from the server  
        $http.get('/home/GetAllLocation').then(function (data) {
             
        $scope.locations = data.data;
         
        for (var i = 0; i < data.data.length; i++)
        {
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

        }, function () {
            alert('Error');
        });

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
         
    });
})();