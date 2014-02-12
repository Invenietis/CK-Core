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

    // Auto-hide groups without warnings, errors, fatals
    $(".logGroup").each(function () {
        var errors = $(".warn, .error, .fatal", this);

        if (errors.length == 0) {
            $(this).collapse('hide');
            // Update toggle status
            $(".collapseToggle[href=\"#" + $(this).attr('id') + "\"]").addClass("collapsed");
        }
    });
});

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