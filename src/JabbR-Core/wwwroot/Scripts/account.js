(function ($) {
    $('input.error').closest('.control-group').addClass('error');

    $('form').each(function () {
        var $el = $(this),
            action = $el.attr('action');

        if (action.indexOf('#') === -1) {
            $el.attr('action', action + document.location.hash);
        }
    });

    $('a').each(function () {
        var $el = $(this),
            href = $el.attr('href');

        if (href.indexOf('#') === -1) {
            if ($el.is('.btn-provider')) {
                href = href + encodeURIComponent(document.location.hash);
            }

            $el.attr('href', href + document.location.hash);
        }
    });
}(jQuery));