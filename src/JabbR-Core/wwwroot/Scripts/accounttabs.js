(function ($) {
    // set the selected tab based on the hash
    var currentSection = document.location.hash || "#profile";
    $('a[href="' + currentSection + '"]').tab('show');

    // always set the hash when we change tabs
    $('a[data-toggle="tab"]').on('shown', function (e) {
        document.location.hash = e.target.hash;
    });

    // make sure errors look pretty and bootstrap-y
    $('input.error').closest('.control-group').addClass('error');
}(jQuery));