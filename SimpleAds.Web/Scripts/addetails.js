$(function() {
    $(".thumb").on('click', function() {
        var src = $(this).attr('src');
        $(".main").attr('src', src);
    });
})