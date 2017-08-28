/// <binding AfterBuild='default' Clean='clean' />

var gulp = require('gulp');
var del = require('del');

var paths = {
    scripts: ['scripts/**/*.js', 'scripts/**/*.ts', 'scripts/**/*.map'],
    dest: 'wwwroot/js'
};

gulp.task('clean', function () {
    return del([paths.dest]);
});

gulp.task('default', function () {
    gulp.src(paths.scripts).pipe(gulp.dest(paths.dest));
});
