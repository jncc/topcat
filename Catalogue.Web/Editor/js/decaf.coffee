

# doing some angular dependency injection!
editorModule = angular.module('editor', []);

editorModule.factory('defaults', () -> {
        name: 'John Smit',
        line1: '123 Main St.',
        city: 'Anytown',
        state: 'AA',
        zip: '12345',
        phone: '1(234) 555-1212' })



