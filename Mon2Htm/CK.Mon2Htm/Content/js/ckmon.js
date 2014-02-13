$(function () {
    $("[rel='tooltip']").tooltip({ html: true, placement: 'auto top' });
    $(".showOnHover").css({ display: 'none' });
    $("div.logLine").hover(
        function (e) {
            showLogLineGlyphs(this);
        },
        function (e) {
            hideLogLineGlyphs(this);
        });

    $('.logGroup.collapse').on('shown.bs.collapse', function (e) {
        processLineClasses();
    });
    $('.logGroup.collapse').on('hidden.bs.collapse', function (e) {
        processLineClasses();
    });
});

function ellipseElement(elementToEllipse) {


}

function getExcerpt(text) {

}

function processLineClasses() {
    // This reprocesses ALL lines. Might want to optimize later.
    var logLineElements = $('div.logLine');

    var reallyvisible = function (a) { return !($(a).is(':hidden') || $(a).parents(':hidden').length || $(a).parents('.collapsing').length || $(a).parents('.collapsed').length) };

    var isEven = true;

    for(var i = 0; i < logLineElements.length; i++) {
        var logLine = $(logLineElements[i]);

        if (reallyvisible(logLine)) {
            if (isEven) { logLine.removeClass("odd").addClass("even"); }
            else { logLine.removeClass("even").addClass("odd"); }

            isEven = !isEven;
        }
    }
}

function hideLogLineGlyphs(element) {
    $(".showOnHover", element).stop(true, true);
    $(".showOnHover", element).fadeOut(100);
}

function showLogLineGlyphs(element) {
    $(".showOnHover", element).stop(true, true);
    $(".showOnHover", element).fadeIn(100);
}

$(document).ready(function () {

    /* Entry page */
    // Enable "More" ellipse for blocks with .longEntry
    $(".longEntry").readmore({
        speed: 400,
        maxHeight: 40,
        sectionCSS: 'display: inline-block;'
    });

    // Hide glyphicons on group messages, and set them to show on hover

    /* Monitor list page */
    $(".monitorEntry").each(function () {
        var startTime = $(".startTime", this).text();
        var endTime = $(".endTime", this).text();

        var mStartTime = moment.utc(startTime);
        var mEndTime = moment.utc(endTime);
        var mEndTime2 = mEndTime.subtract(mStartTime);

        $(".startTime", this).text(mStartTime.fromNow());
        $(".endTime", this).text(moment.duration(mEndTime2).humanize());
    });


});