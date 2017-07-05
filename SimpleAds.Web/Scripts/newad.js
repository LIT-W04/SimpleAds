$(function() {
    var imageIndex = 0;
    $(".add-image").on('click', function() {
        $("form").append("<input type='file' name='adimages[" + imageIndex + "]' /> <br />");
        imageIndex++;
    });
});