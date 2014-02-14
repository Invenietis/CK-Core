$(function () {
    window.freezeReprocessing = false;
    var $lastClickedLogLine;
    var $contextMenu = $('#contextMenu');

    // Prepare context menu
    $("body").on("contextmenu", ".logLine", function (e) {
        $rowClicked = $(this)
        $contextMenu.css({
            display: "block",
            left: e.pageX,
            top: e.pageY
        });
        return false;
    });

    $contextMenu.on("click", "a", function () {
        switch ($(this).attr('id')) {
            case 'expandGroupsMenuEntry':
                window.freezeReprocessing = true;
                expandGroups();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'expandAllMenuEntry':
                window.freezeReprocessing = true;
                expandEverything();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'collapseGroupsMenuEntry':
                window.freezeReprocessing = true;
                collapseGroups();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'collapseAllMenuEntry':
                window.freezeReprocessing = true;
                collapseEverything();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            default:
        }

        $contextMenu.hide();
    });
    $(document).click(function () {
        $contextMenu.hide();
    });

    // Init tooltips
    $("[rel='tooltip']").tooltip({ html: true, placement: 'auto top' });

    // Init hover ffw/fbw buttons
    $(".showOnHover").css({ display: 'none' });
    $("div.logLine").hover(
        function (e) {
            showLogLineGlyphs(this);
        },
        function (e) {
            hideLogLineGlyphs(this);
        });

    // Init line processing on group show/hide
    $('.logGroup.collapse').on('shown.bs.collapse', function (e) {
        processLineClasses();
    });
    $('.logGroup.collapse').on('hidden.bs.collapse', function (e) {
        processLineClasses();
    });

    // Init long entry collapsing
    $(".longEntry").each(function () { ellipseElement(this); });

    // Process even/odd once
    processLineClasses();
});

function ellipseElement(elementToEllipse) {
    var text = $(elementToEllipse).html();
    $(elementToEllipse).data("fullText", htmlEncode(text));

    collapseEllipse(elementToEllipse);
}

function collapseEllipse(element) {
    var $element = $(element);
    if ($element.hasClass("collapsed")) { return; }

    var text = getHtmlExcerpt($element.text());

    var $link = $('<a href="#">•••</a>');
    $link.click(function (e) {
        expandEllipse($element);
        return false;
    });

    $element.html(text);
    $element.append($link);
    $element.addClass("collapsed");
}

function expandEllipse(element) {
    var $element = $(element);
    if (!$element.hasClass("collapsed") || typeof $element.data("fullText") === 'undefined') { return; }

    var text = $element.data("fullText");

    var $link = $('<a href="#">•••</a>');
    $link.click(function (e) {
        collapseEllipse(element);
        return false;
    });

    $element.html(htmlDecode(text));
    $element.append($link);
    $element.removeClass("collapsed");
}

function collapseGroups() {
    $(".logGroup.in").each(function () {
        //$(this).collapse('hide');
        $(this).removeClass("in");
        // Update toggle status
        $(".collapseToggle[href=\"#" + $(this).attr('id') + "\"]").addClass("collapsed");
    });
}

function expandGroups() {
    $(".logGroup:not(.in)").each(function () {
        //$(this).collapse('show');
        $(this).addClass("in");
        // Update toggle status
        $(".collapseToggle[href=\"#" + $(this).attr('id') + "\"]").removeClass("collapsed");
    });
}

function collapseEverything() {
    collapseGroups();

    $(".longEntry").each(function () {
        collapseEllipse($(this));
    });

    $(".exceptionContainer.in").each(function () {
        $(this).removeClass("in");
        //$(this).collapse('hide');
    });
}

function expandEverything() {
    expandGroups();

    $(".longEntry").each(function () {
        expandEllipse($(this));
    });

    $(".exceptionContainer:not(.in)").each(function () {
        //$(this).collapse('show');
        $(this).addClass("in");
    });
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

function getHtmlExcerpt(text) {
    return $('<div/>').text(text.replace(/(\r+\n+|\n+|\r+)/gm, "↵").replace(/\t+/gm, ' ').replace(/\s\s+/gm, ' ').substr(0, 100)).html().replace(/↵/gm, '<span class="newLineIcon">↵</span>');
}

function processLineClasses() {
    if (window.freezeReprocessing) return;
    // This reprocesses ALL lines. Might want to optimize later.
    var logLineElements = $('div.logLine');

    var reallyvisible = function (a) { return !($(a).is(':hidden') || $(a).parents(':hidden').length || $(a).parents('.collapsing').length || $(a).parents('.collapsed').length) };

    var isEven = true;

    for (var i = 0; i < logLineElements.length; i++) {
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
    /* Monitor list page */
    $(".monitorEntry").each(function () {
        var startTime = $(".startTime", this).text();
        var endTime = $(".endTime", this).text();

        var mStartTime = moment.utc(startTime);
        var mEndTime = moment.utc(endTime);

        var startValue = mStartTime.valueOf();
        var endValue = mEndTime.valueOf();

        var durationValue = endValue - startValue;
        var mDuration = moment.duration(durationValue, 'ms');

        $(".startTime", this).text(mStartTime.fromNow());
        $(".endTime", this).text(mDuration.humanize());
    });


});