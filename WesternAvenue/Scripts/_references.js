/// <autosync enabled="true" />
/// <reference path="_map.js" />
/// <reference path="angular.min.js" />
/// <reference path="angular-google-maps.min.js" />
/// <reference path="angular-google-maps-street-view.min.js" />
/// <reference path="angular-mocks.js" />
/// <reference path="bootstrap.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="jquery-1.10.2.js" />
/// <reference path="lodash.min.js" />
/// <reference path="modernizr-2.6.2.js" />
/// <reference path="respond.js" />
function isMobile() {
    if (window.innerWidth <= 800 && window.innerHeight <= 600) {
        return true;
    } else {
        return false;
    }
}

function pageLoad()
{
    console.log('loaded');
}