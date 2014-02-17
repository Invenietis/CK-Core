$(function () {
    window.freezeReprocessing = false;
    window.$lastClickedLogLine = "";
    var $contextMenu = $('#contextMenu');
    window.$selectedHtml = "";

    // Prepare context menu
    $("body").on("contextmenu", ".logLine", function (e) {
        window.$lastClickedLogLine = $(this);
        window.$lastClickedLogLine.addClass("selectedLine");
        window.$selectedHtml = $(getHTMLOfSelection());

        if (typeof window.$selectedHtml == 'undefined' || window.$selectedHtml === "" || window.$selectedHtml.length == 0) {
            $(".needsSelection", $contextMenu).css('display', 'none');
            $(".needsNoSelection", $contextMenu).css('display', 'block');
        } else {
            $(".needsSelection", $contextMenu).css('display', 'block');
            $(".needsNoSelection", $contextMenu).css('display', 'none');
        }

        var $group = getGroupOfHeaderGroupLine(window.$lastClickedLogLine);

        if($group.length == 0) {
            $(".needsGroupHeader", $contextMenu).css('display', 'none');
        } else {
            updateToggleGroupMenuEntry(window.$lastClickedLogLine);
            $(".needsGroupHeader", $contextMenu).css('display', 'block');
        }

        var $parentGroup = getParentGroupOfLine(window.$lastClickedLogLine);

        if ($parentGroup !== null) {
            updateCloseParentMenuEntry($parentGroup);
            $(".needsParentGroup", $contextMenu).css('display', 'block');
        } else {
            $(".needsParentGroup", $contextMenu).css('display', 'none');
        }

        $contextMenu.css({
            display: "block",
            left: e.pageX,
            top: e.pageY
        });
        return false;
    });

    $contextMenu.on("click", "a", function (event) {
        switch ($(this).attr('id')) {
            case 'toggleGroupMenuEntry':
                window.freezeReprocessing = true;
                var $group = $(getGroupOfHeaderGroupLine(window.$lastClickedLogLine));
                if ($group.hasClass("in")) {
                    // Group is open
                    collapseGroup($group);
                } else {
                    // Group is closed
                    expandGroup($group);
                }
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'collapseParentMenuEntry':
                window.freezeReprocessing = true;

                var $group = $(getParentGroupOfLine(window.$lastClickedLogLine));
                if ($group !== null) {
                    // Group is closed
                    collapseGroup($group);
                }

                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'expandGroupsMenuEntry':
                window.freezeReprocessing = true;
                expandGroups();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'expandContentMenuEntry':
                expandContent();
                break;
            case 'collapseGroupsMenuEntry':
                window.freezeReprocessing = true;
                collapseGroups();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'collapseContentMenuEntry':
                collapseContent();
                break;
            case 'collapseSelectionMenuEntry':
                window.freezeReprocessing = true;
                collapseSelection();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            case 'expandSelectionMenuEntry':
                window.freezeReprocessing = true;
                expandSelection();
                window.freezeReprocessing = false;
                processLineClasses();
                break;
            default:
        }

        $contextMenu.hide();
        event.preventDefault();
        $(".selectedLine").each(function () { $(this).removeClass("selectedLine"); });
        return false;
    });
    $(document).click(function () {
        $contextMenu.hide();
        $(".selectedLine").each(function () { $(this).removeClass("selectedLine"); });
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

function updateToggleGroupMenuEntry($clickedLogLine) {
    var text = "";
    if ($clickedLogLine.find(".collapseToggle").hasClass("collapsed")) {
        // Group is closed
        text = "Open \"" + $clickedLogLine.find(".collapseTitle").text() + "\"";
    } else {
        text = "Close \"" + $clickedLogLine.find(".collapseTitle").text() + "\"";
    }
    $("#toggleGroupMenuEntry").text(text);
}

function updateCloseParentMenuEntry($group) {
    var text = "Close parent: \"" + getTitleOfGroup($group).text() + "\"";

    $("#collapseParentMenuEntry").text(text);
}

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

function getParentGroupOfLine(groupLine) {
    var $groupLine = $(groupLine);
    var $groupParent = $groupLine.parent(".logGroup");
    if ($groupParent.length == 0) return null;
    return $groupParent;
}

function getGroupOfHeaderGroupLine(groupLineElement)
{
    var $line = $(groupLineElement);
    return getGroupOfTitle($line.find("a.collapseToggle"));
}

function getTitleOfGroup(groupElement) {
    var $groupElement = $(groupElement);
    return $(".collapseToggle[href=\"#" + $groupElement.attr('id') + "\"]")
}
function getGroupOfTitle(collapseTitle) {
    var href = $(collapseTitle).attr('href');
    var $groupDiv = $(href);

    return $groupDiv;
}

function collapseGroups() {
    $(".logGroup").each(function () {
        collapseGroup(this);
    });
}

function collapseGroup(groupElement) {
    var $groupElement = $(groupElement);
    $groupElement.removeClass("in");
    $groupElement.css("height", "0px");
    $groupElement.css("overflow", "hidden");
    // Update toggle status
    getTitleOfGroup(groupElement).addClass("collapsed");
}

function expandGroups() {
    $(".logGroup").each(function () {
        expandGroup(this);
    });
}

function expandGroup(groupElement) {
    var $groupElement = $(groupElement);
    $groupElement.addClass("in");
    $groupElement.css("height", "");
    $groupElement.css("overflow", "");
    // Update toggle status
    getTitleOfGroup(groupElement).removeClass("collapsed");
}

function collapseContent() {

    $(".longEntry").each(function () {
        collapseEllipse($(this));
    });

    $(".exceptionContainer").each(function () {
        $(this).removeClass("in");
        $(this).css("height", "0px");
        $(this).css("overflow", "hidden");
    });
}

function expandContent() {
    $(".longEntry").each(function () {
        expandEllipse($(this));
    });

    $(".exceptionContainer").each(function () {
        $(this).addClass("in");
        $(this).css("height", "");
        $(this).css("overflow", "");
    });
}

function collapseSelection() {
    if (typeof window.$selectedHtml == 'undefined' || window.$selectedHtml === "" || window.$selectedHtml == 0) return;
    getLogGroupsInSelection().forEach(function (logGroupElement) {
        collapseGroup(logGroupElement);
    });
}

function expandSelection() {
    if (typeof window.$selectedHtml == 'undefined' || window.$selectedHtml === "" || window.$selectedHtml == 0) return;
    getLogGroupsInSelection().forEach(function (logGroupElement) {
        expandGroup(logGroupElement);
    });
}

function getLogGroupsInSelection() {
    // Add filter (all at root)
    var $selectedLogGroups = window.$selectedHtml.filter(".logGroup").add(window.$selectedHtml.find(".logGroup"));

    // Create group array
    var logGroupArray = $selectedLogGroups.map(function () { return $(document.getElementById(this.id)); }).get();

    // Add missing groups where we have header
    var $selectedLogHeaders = $(window.$selectedHtml).filter(".collapseTitle");
    $selectedLogHeaders.each(function (groupTitle) {
        $group = getGroupOfTitle(groupTitle);
        if ($.inArray($group, logGroupArray) === -1) {
            logGroupArray.push($group);
        }
    });

    return logGroupArray;
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

function getHTMLOfSelection() {
    var range;
    if (document.selection && document.selection.createRange) {
        range = document.selection.createRange();
        return range.htmlText;
    }
    else if (window.getSelection) {
        var selection = window.getSelection();
        if (selection.rangeCount > 0) {
            range = selection.getRangeAt(0);
            var clonedSelection = range.cloneContents();
            var div = document.createElement('div');
            div.appendChild(clonedSelection);
            return div.innerHTML;
        }
        else {
            return '';
        }
    }
    else {
        return '';
    }
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