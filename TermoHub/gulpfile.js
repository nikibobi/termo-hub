/// <binding AfterBuild='default' Clean='clean' />

var gulp = require('gulp');
var changed = require('gulp-changed');
var del = require('del');

var paths = {
    scripts: ['scripts/**/*.js', 'scripts/**/*.ts', 'scripts/**/*.map'],
    dest: 'wwwroot/js'
};

gulp.task('clean', function () {
    return del([paths.dest]);
});

gulp.task('default', function () {
    gulp.src(paths.scripts)
        .pipe(changed(paths.dest))
        .pipe(gulp.dest(paths.dest));
});
