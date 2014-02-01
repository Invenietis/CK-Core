$(function () {
    $("[rel='tooltip']").tooltip({ html: true, placement: 'auto top' });
});

$(document).ready(function () {

    $("pre.logLine").readmore({
        speed: 400,
        maxHeight: 40
    });

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