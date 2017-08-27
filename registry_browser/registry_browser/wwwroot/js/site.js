function copyToClipboard(text) {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val(text).select();
    document.execCommand("copy");
    $temp.remove();
}

$( document ).ready(function() {
    $(".rv-copybutton").each(function( index ) {
        $(this).click(function() {
            copyToClipboard($(this).data("copy"));
          });
      });
});