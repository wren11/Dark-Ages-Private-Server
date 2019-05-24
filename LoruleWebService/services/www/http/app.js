var sampleApp = angular.module('sampleApp', ['ngRoute']);

sampleApp.config(['$routeProvider', '$locationProvider',
	function ($routeProvider, $locationProvider) {
		$routeProvider.
			when('/ShowOrder/:orderId', {
				templateUrl: '%base_dir%/view_user.html',
				controller: 'ShowOrderController'
			});

		$routeProvider
			.when('/deleteItem/:id', {
				controller: 'InvokeController',
				template: "{{template}}"
			});

		$routeProvider
			.when('/users', {
				controller: 'serverController',
				template: "{{OnlineUsers}}"
			});
		$routeProvider
			.when('/logs', {
				controller: 'logController',
				template: "{{Logs}}"
			});
		$routeProvider
			.when('/items', {
				controller: 'itemsController',
				templateUrl: "views/items.html"
			});

		$locationProvider.html5Mode(true);
	}]);

sampleApp.controller('ServerController', function ($scope, $routeParams, $http) {
	$scope.getUsers = function () {
		$http({
			method: "GET",
			url: "api/users.html?all"
		}).then(function mySucces(response) {
			$scope.OnlineUsers = response.data;
		});
    };
    $scope.getServerStatus = function () {
        $http({
            method: "GET",
            url: "api/status.html"
        }).then(function mySucces(response) {
            $scope.ServerStatus = response.data;
        });
	};
});

sampleApp.controller('LogController', function ($scope, $routeParams, $http) {
	$scope.getServerLogs = function () {
		$http({
			method: "GET",
			url: "api/logs.html"
		}).then(function mySucces(response) {
			$scope.Logs = response.data;
		});
	};
});


sampleApp.controller('MenuController', ['$route', '$location', function ($route, $routeParams, $location) {
	this.$route = $route;
	this.$location = $location;
	this.$routeParams = $routeParams;
}]).controller('itemsController', ['$routeParams', function ItemsController($routeParams) {
	this.name = 'itemsController';
	this.params = $routeParams;
}]);

sampleApp.controller('InvokeController', function ($scope, $routeParams, $http) {

});