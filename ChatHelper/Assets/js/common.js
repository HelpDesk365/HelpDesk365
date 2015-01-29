﻿/*
 * jQuery dropdown: A simple dropdown plugin
 *
 * Copyright A Beautiful Site, LLC. (http://www.abeautifulsite.net/)
 *
 * Licensed under the MIT license: http://opensource.org/licenses/MIT
 *
*/jQuery && function (e) { function t(t, i) { var s = t ? e(this) : i, o = e(s.attr("data-dropdown")), u = s.hasClass("dropdown-open"); if (t) { if (e(t.target).hasClass("dropdown-ignore")) return; t.preventDefault(); t.stopPropagation() } else if (s !== i.target && e(i.target).hasClass("dropdown-ignore")) return; n(); if (u || s.hasClass("dropdown-disabled")) return; s.addClass("dropdown-open"); o.data("dropdown-trigger", s).show(); r(); o.trigger("show", { dropdown: o, trigger: s }) } function n(t) { var n = t ? e(t.target).parents().addBack() : null; if (n && n.is(".dropdown")) { if (!n.is(".dropdown-menu")) return; if (!n.is("A")) return } e(document).find(".dropdown:visible").each(function () { var t = e(this); t.hide().removeData("dropdown-trigger").trigger("hide", { dropdown: t }) }); e(document).find(".dropdown-open").removeClass("dropdown-open") } function r() { var t = e(".dropdown:visible").eq(0), n = t.data("dropdown-trigger"), r = n ? parseInt(n.attr("data-horizontal-offset") || 0, 10) : null, i = n ? parseInt(n.attr("data-vertical-offset") || 0, 10) : null; if (t.length === 0 || !n) return; t.hasClass("dropdown-relative") ? t.css({ left: t.hasClass("dropdown-anchor-right") ? n.position().left - (t.outerWidth(!0) - n.outerWidth(!0)) - parseInt(n.css("margin-right"), 10) + r : n.position().left + parseInt(n.css("margin-left"), 10) + r, top: n.position().top + n.outerHeight(!0) - parseInt(n.css("margin-top"), 10) + i }) : t.css({ left: t.hasClass("dropdown-anchor-right") ? n.offset().left - (t.outerWidth() - n.outerWidth()) + r : n.offset().left + r, top: n.offset().top + n.outerHeight() + i }) } e.extend(e.fn, { dropdown: function (r, i) { switch (r) { case "show": t(null, e(this)); return e(this); case "hide": n(); return e(this); case "attach": return e(this).attr("data-dropdown", i); case "detach": n(); return e(this).removeAttr("data-dropdown"); case "disable": return e(this).addClass("dropdown-disabled"); case "enable": n(); return e(this).removeClass("dropdown-disabled") } } }); e(document).on("click.dropdown", "[data-dropdown]", t); e(document).on("click.dropdown", n); e(window).on("resize", r) }(jQuery);

function chatBodyHeight() {
    var newHeight = $(window).height() - 270;
    $('.chat-content-body').css('height', newHeight);
}
function chatWindowScroll() {
    var cwh = $('.chat-content-body').height();
    if ($('.chat-content-body').length > 0) {
        $('.chat-content-body').scrollTop(cwh);
    }
}

$(function () {
    chatBodyHeight();
    chatWindowScroll();
    $(window).resize(function () {
        chatBodyHeight();
        chatWindowScroll();
    });
});















