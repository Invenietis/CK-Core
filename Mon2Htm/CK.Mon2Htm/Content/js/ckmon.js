$(function () {
    $("[rel='tooltip']").tooltip({ html: true, placement: 'auto top' });
});

$(document).ready(function () {

    /* Entry page */
    $(".longEntry").readmore({
        speed: 400,
        maxHeight: 40,
        sectionCSS: 'display: inline-block;'
    });

    $(".logGroup").each(function () {
        var errors = $(".warn, .error, .fatal", this);

        if( errors.length == 0 )
        {
            $(this).collapse('hide');
            // Update toggle status
            $(".collapseToggle[href=\"#"+$(this).attr('id')+"\"]").addClass("collapsed");
        }
    });

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