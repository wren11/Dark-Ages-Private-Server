var sampleApp = angular.module('sampleApp', []);

sampleApp.config(['$routeProvider',
	function ($routeProvider) {
		$routeProvider.
			when('/ShowOrder/:orderId', {
				templateUrl: '%base_dir%/view_user.html',
				controller: 'ShowOrderController'
			});

		$routeProvider
			.when('/deleteItem/:id', {
				controller: 'InvokeController',
				template: "{{template}}",
			});

		$routeProvider
			.when('/reboot', {
				controller: 'serverController',
				template: "{{server_status}}",
			});

		$routeProvider
			.when('/users', {
				controller: 'serverController',
				template: "{{OnlineUsers}}",
			});
	}]);

sampleApp.controller('ServerController', function ($scope, $routeParams, $http) {
	$scope.stats = "%stats%";
	$scope.go = function() {
		$http({
			method: "GET",
			url: "api/reboot.html?act=5",
		}).then(function mySucces(response) {
			$scope.server_status = response;
		});
	};
	$scope.getUsers = function () {
		$http({
			method: "GET",
			url: "api/users.html?all",
		}).then(function mySucces(response) {
			$scope.OnlineUsers = response.data;
		});
	};
});

sampleApp.controller('InvokeController', function ($scope, $routeParams, $http) {

});