$(document).on('click', '.clinic-menu-toggle', function (e) {

    e.preventDefault();

    var parent = $(this).closest('.clinic-menu-parent');

    parent.toggleClass('is-open');

});