$(function () {

    window.freezeReprocessing = false;
    window.$lastClickedLogLine = "";
    window.$contextMenu = $('#contextMenu');
    window.$selectedHtml = "";

    window.visibleLogLines = [];
    window.selectionStart = -1;
    window.selectionEnd = -1;

    window.selectedLines = [];

    window.shifted = false;

    $(document).bind('keyup keydown', function (e) { window.shifted = e.shiftKey; return true; });

    $('.logLine').mousedown(function (event) {
        switch (event.which) {
            case 3:
                startLogEntrySelection(this);
                break;
        }
    });

    $('.logLine').mouseup(function (event) {
        switch (event.which) {
            case 3:
                stopLogEntrySelection(this);
                break;
        }
    });

    // Header menu entries
    $("#openAllGroupsMenuEntry").on("click", function (event) {
        window.freezeReprocessing = true;
        expandAllGroups();
        window.freezeReprocessing = false;
        processLineClasses();
        event.preventDefault();
        return false;
    });
    $("#closeAllGroupsMenuEntry").on("click", function (event) {
        window.freezeReprocessing = true;
        collapseAllGroups();
        window.freezeReprocessing = false;
        processLineClasses();
        event.preventDefault();
        return false;
    });
    $("#openAllContentMenuEntry").on("click", function (event) {
        window.freezeReprocessing = true;
        expandAllContent();
        window.freezeReprocessing = false;
        processLineClasses();
        event.preventDefault();
        return false;
    });
    $("#closeAllContentMenuEntry").on("click", function (event) {
        window.freezeReprocessing = true;
        collapseAllContent();
        window.freezeReprocessing = false;
        processLineClasses();
        event.preventDefault();
        return false;
    });

    // Prepare context menu
    $("body").on("contextmenu", ".logLine", function (e) {
        if (!window.shifted) {
            return createContextMenu(e);
        } else {
            return true;
        }
    });

    $contextMenu.on("click", "a", function (event) {
        return onContextMenuEntryClick(event, $(this));
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

function getSelectedLinesCount() {
    if (window.selectionEnd == -1 || window.selectionStart == -1) return 0;
    return Math.abs(window.selectionEnd - window.selectionStart) + 1;
}

function getSelectedLines() {
    return window.visibleLogLines.slice(window.selectionStart, window.selectionEnd+1);
}

function createContextMenu(e) {
    var $contextMenu = $('#contextMenu');
    var selectedLineCount = getSelectedLinesCount();
    var selectedLines = [];
    if (selectedLineCount > 0) selectedLines = getSelectedLines();
    window.selectedLines = selectedLines;

    if (selectedLineCount > 1 && hasCollapsedGroupsInSelection(selectedLines)) {
        $("#expandStructureMenuEntry", $contextMenu).css('display', 'block');
    } else {
        $("#expandStructureMenuEntry", $contextMenu).css('display', 'none');
    }

    if (selectedLineCount > 1 && hasOpenGroupsInSelection(selectedLines)) {
        $("#collapseStructureMenuEntry", $contextMenu).css('display', 'block');
    } else {
        $("#collapseStructureMenuEntry", $contextMenu).css('display', 'none');
    }

    if (hasCollapsedContentInSelection(selectedLines)) {
        $("#expandContentMenuEntry", $contextMenu).css('display', 'block');
    } else {
        $("#expandContentMenuEntry", $contextMenu).css('display', 'none');
    }

    if (hasOpenContentInSelection(selectedLines)) {
        $("#collapseContentMenuEntry", $contextMenu).css('display', 'block');
    } else {
        $("#collapseContentMenuEntry", $contextMenu).css('display', 'none');
    }

    if (selectedLineCount == 1) {
        // If single selection is a group header
        var $group = getGroupOfHeaderGroupLine(selectedLines[0]);
        if ($group.length == 0) {
            $("#toggleGroupMenuEntry", $contextMenu).css('display', 'none');
        } else {
            updateToggleGroupMenuEntry(selectedLines[0]);
            $("#toggleGroupMenuEntry", $contextMenu).css('display', 'block');
        }

        // If single selection has a group parent
        var $parentGroup = getParentGroupOfLine(selectedLines[0]);
        if ($parentGroup !== null) {
            updateCloseParentMenuEntry($parentGroup);
            $("#collapseParentMenuEntry", $contextMenu).css('display', 'block');
        } else {
            $("#collapseParentMenuEntry", $contextMenu).css('display', 'none');
        }
    } else {
        $("#collapseParentMenuEntry", $contextMenu).css('display', 'none');
        $("#toggleGroupMenuEntry", $contextMenu).css('display', 'none');
    }

    $contextMenu.css({
        display: "block",
        left: e.pageX,
        top: e.pageY
    });
    return false;
}

function getVisibleLogLines() {
    var visibleLogLines = [];
    var allLogLines = $('.logLine:visible');

    var isLineVisible = function (a) {
        return !($(a).is(':hidden') || $(a).parents(':hidden').length || $(a).parents('.collapsing').length || $(a).parents('.collapsed').length)
    };

    for (var i = 0; i < allLogLines.length; i++) {
        var $logLine = $(allLogLines[i]);

        if (isLineVisible($logLine)) {
            visibleLogLines.push($logLine.get(0));
        }
    }
    return visibleLogLines;
}

function onContextMenuEntryClick(event, clickedLink) {
    var $clickedLink = $(clickedLink);
    var $contextMenu = $('#contextMenu');
    switch ($clickedLink.attr('id')) {
        case 'toggleGroupMenuEntry':
            window.freezeReprocessing = true;
            var $group = $(getGroupOfHeaderGroupLine(window.selectedLines[0]));
            if ($group.hasClass("in")) {
                // Group is open
                collapseGroup($group, true);
            } else {
                // Group is closed
                expandGroup($group, true);
            }
            window.freezeReprocessing = false;
            processLineClasses();
            break;

        case 'collapseParentMenuEntry':
            window.freezeReprocessing = true;

            var $group = $(getParentGroupOfLine(window.selectedLines[0]));
            if ($group !== null) {
                collapseGroup($group);
            }

            window.freezeReprocessing = false;
            processLineClasses();
            break;

        case 'expandStructureMenuEntry':
            window.freezeReprocessing = true;
            expandSelectedStructure();
            window.freezeReprocessing = false;
            processLineClasses();
            break;
        case 'collapseStructureMenuEntry':
            window.freezeReprocessing = true;
            collapseSelectedStructure();
            window.freezeReprocessing = false;
            processLineClasses();
            break;

        case 'expandContentMenuEntry':
            expandSelectedContent();
            break;
        case 'collapseContentMenuEntry':
            collapseSelectedContent();
            break;
        default:
    }

    $contextMenu.hide();
    event.preventDefault();
    resetSelection();
    return false;
}

function hasCollapsedGroupsInSelection(selectedLines) {
    var hasCollapsedGroups = false;
    selectedLines.forEach(function (logLine) {
        if (hasCollapsedGroups) return;
        if ($(logLine).find(".collapseTitle.collapsed").length > 0) hasCollapsedGroups = true;
    });
    return hasCollapsedGroups;
}

function hasOpenGroupsInSelection(selectedLines) {
    var hasOpenGroups = false;
    selectedLines.forEach(function (logLine) {
        if (hasOpenGroups) return;
        if ($(logLine).find(".collapseTitle:not(.collapsed)").length > 0) hasOpenGroups = true;
    });
    return hasOpenGroups;
}

function hasCollapsedContentInSelection(selectedLines) {
    var hasCollapsedContent = false;
    selectedLines.forEach(function (logLine) {
        if (hasCollapsedContent) return;
        if ($(logLine).find(".longEntry.collapsed").length > 0) hasCollapsedContent = true;
        if ($(logLine).find(".exceptionContainer:not(.in)").length > 0) hasCollapsedContent = true;
    });
    return hasCollapsedContent;
}

function hasOpenContentInSelection(selectedLines) {
    var hasOpenContent = false;
    selectedLines.forEach(function (logLine) {
        if (hasOpenContent) return;
        if ($(logLine).find(".longEntry:not(.collapsed)").length > 0) hasOpenContent = true;
        if ($(logLine).find(".exceptionContainer.in").length > 0) hasOpenContent = true;
    });
    return hasOpenContent;
}

function resetSelection() {
    window.selectionStart = -1;
    window.selectionEnd = -1;
    $(".selectedLine").each(function () { $(this).removeClass("selectedLine"); });
}

function startLogEntrySelection(logLine) {
    resetSelection();

    window.visibleLogLines = getVisibleLogLines();

    window.selectionStart = $.inArray($(logLine).get(0), window.visibleLogLines);
    window.selectionEnd = window.selectionStart;
    if (window.selectionStart == -1) console.error("Selected line not found in visible lines array.");

    selectLogLine(logLine);

    $(".logLine").each(function () { var logLine = $(this); logLine.bind("mouseenter", handleLogEntryMouseEnter); });
}

function stopLogEntrySelection(logLine) {
    $(".logLine").each(function () { var logLine = $(this); logLine.unbind("mouseenter", handleLogEntryMouseEnter); });
}

function handleLogEntryMouseEnter(e) {
    selectLogLine(this);
}

function selectLogLine(logLine) {


    var previousSelectionEndIndex = window.selectionEnd;

    window.selectionEnd = $.inArray($(logLine).get(0), window.visibleLogLines);


    console.log("Start: " + window.selectionStart + " - End: " + window.selectionEnd + " - Previous: " + previousSelectionEndIndex);

    if (window.selectionEnd == -1) {
        console.error("Selected line not found in visible lines array.");
    } else {
        if (previousSelectionEndIndex == -1) { previousSelectionEndIndex = window.selectionStart; }

        var i = previousSelectionEndIndex;

        while (true) {
            var selectedLogLine = $(window.visibleLogLines[i]);
            console.log("Processing index " + i);

            if (window.selectionEnd > window.selectionStart) {

                if (i > window.selectionEnd) {
                    console.log("Deselected index " + i);
                    selectedLogLine.removeClass("selectedLine");
                } else {
                    console.log("Selected index " + i);
                    selectedLogLine.addClass("selectedLine");
                }

            } else if (window.selectionEnd < window.selectionStart) {
                if (i < window.selectionEnd) {
                    console.log("Deselected index " + i);
                    selectedLogLine.removeClass("selectedLine");
                } else {
                    console.log("Selected index " + i);
                    selectedLogLine.addClass("selectedLine");
                }
            } else {
                // start == end
                if (i != window.selectionEnd) {
                    console.log("Deselected index " + i);
                    selectedLogLine.removeClass("selectedLine");
                } else {
                    console.log("Selected index " + i);
                    selectedLogLine.addClass("selectedLine");
                }
            }

            if (i < window.selectionEnd) {
                i++;
            } else if (i > window.selectionEnd) {
                i--;
            } else {
                break;
            }
        }

    }
}

function isInSelectionArray(index) {
    if (window.selectionStart == -1 || window.selectionEnd == -1) return false;

    if (window.selectionStart > window.selectionEnd) {
        return (index >= window.selectionEnd && index <= window.selectionStart);
    } else {
        return (index >= window.selectionStart && index <= window.selectionEnd);
    }
}

function deselectLogLine(logLine) {
    var $logLine = $(logLine);

    if ($.inArray($logLine, window.selectedLogLines) !== -1) {
        window.selectedLogLines.splice($.inArray($logLine, window.selectedLogLines), 1);

        $logLine.removeClass("selectedLine");
    }
}

function updateToggleGroupMenuEntry(clickedLogLine) {
    var text = "";
    var $clickedLogLine = $(clickedLogLine);
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

function getGroupOfHeaderGroupLine(groupLineElement) {
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

// On single elements

function collapseGroup(groupElement, recurse) {
    recurse = typeof recurse !== 'undefined' ? recurse : false;

    var $groupElement = $(groupElement);
    if (recurse) {
        $groupElement.find("> .logGroup").each(function () {
            collapseGroup(this, recurse);
        });
    }

    $groupElement.removeClass("in");
    $groupElement.css("height", "0px");
    $groupElement.css("overflow", "hidden");
    // Update toggle status
    getTitleOfGroup(groupElement).addClass("collapsed");
}

function expandGroup(groupElement, recurse) {
    recurse = typeof recurse !== 'undefined' ? recurse : false;

    var $groupElement = $(groupElement);
    $groupElement.addClass("in");
    $groupElement.css("height", "");
    $groupElement.css("overflow", "");
    // Update toggle status
    getTitleOfGroup(groupElement).removeClass("collapsed");

    if (recurse) {
        $groupElement.find("> .logGroup").each(function () {
            expandGroup(this, recurse);
        });
    }
}

// On all elements

function expandAllGroups() {
    $(".logGroup").each(function () {
        expandGroup(this);
    });
}

function collapseAllGroups() {
    $(".logGroup").each(function () {
        collapseGroup(this);
    });
}

function expandAllContent() {
    $(".longEntry").each(function () {
        expandEllipse($(this));
    });

    $(".exceptionContainer").each(function () {
        $(this).addClass("in");
        $(this).css("height", "");
        $(this).css("overflow", "");
    });
}

function collapseAllContent() {

    $(".longEntry").each(function () {
        collapseEllipse($(this));
    });

    $(".exceptionContainer").each(function () {
        $(this).removeClass("in");
        $(this).css("height", "0px");
        $(this).css("overflow", "hidden");
    });
}

// On selection

function expandSelectedStructure() {
    if (window.selectedLines.length == 0) return;

    window.selectedLines.forEach(function (logLine) {
        var $logGroupTitle = $(logLine).find(".collapseTitle");

        if ($logGroupTitle.length > 0) {
            var $group = getGroupOfTitle($logGroupTitle);
            expandGroup($group, true);
        }
    });
}

function collapseSelectedStructure() {
    if (window.selectedLines.length == 0) return;

    window.selectedLines.forEach(function (logLine) {
        var $logGroupTitle = $(logLine).find(".collapseTitle");

        if ($logGroupTitle.length > 0) {
            var $group = getGroupOfTitle($logGroupTitle);
            collapseGroup($group, true);
        }
    });
}

function expandSelectedContent() {
    if (window.selectedLines.length == 0) return;

    window.selectedLines.forEach(function (logLine) {
        $logLine = $(logLine);

        $logLine.find(".longEntry").each(function () {
            expandEllipse($(this));
        });

        $logLine.find(".exceptionContainer").each(function () {
            $(this).addClass("in");
            $(this).css("height", "");
            $(this).css("overflow", "");
        });
    });
}

function collapseSelectedContent() {
    if (window.selectedLines.length == 0) return;

    window.selectedLines.forEach(function (logLine) {
        $(logLine).find(".longEntry").each(function () {
            collapseEllipse($(this));
        });

        $(logLine).find(".exceptionContainer").each(function () {
            $(this).removeClass("in");
            $(this).css("height", "0px");
            $(this).css("overflow", "hidden");
        });
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