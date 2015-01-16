// modal
(function($){
	$.fn.modal = function(options){
		var defaults = {
			show:false,
			overlayDrop:true
		}
		var options = $.extend(defaults, options);
		var $btnOpen = $('[data-toggle="modal"]');
		var modal = this;
		if(options.overlayDrop == true){
			if ($('body').find('.modal-backdrop').length == 0)
			{
				$('body').prepend('<div class="modal-backdrop"></div>');
			}
			var overlay = $('.modal-backdrop');
		}
		function modalShow(){
			$('body').addClass('modal-open');
			modal.show();
			overlay.show();
		}
		if(options.show == true){
			modalShow();
		}
	}
})(jQuery);

function chatWindowScroll(){
	var cwh = $(window).height();
	if ($('.chat-window').length > 0)
	{
		$(window).scrollTop(cwh);
	}
}

$(window).resize(function(){
	chatWindowScroll();
});
$(function(){
	chatWindowScroll();

	$('#header .btn-nav-toggle').click(function(event){
		event.stopPropagation();
		if ($(this).hasClass('active'))
		{
			$(this).removeClass('active');
			$('#header ul').hide();
		} else {
			$(this).addClass('active');
			$('#header ul').show();
		}
		return false;
	});

	$('[data-toggle="modal"]').click(function(){
		var target = $(this).attr('data-target');
		if ($('body').find('.modal-backdrop').length == 0)
		{
			$('body').prepend('<div class="modal-backdrop"></div>');
		}
		$('body').addClass('modal-open');
		$('.modal-backdrop').show();
		$(target).show();
		return false;
	});
	$('[data-dismiss="modal"]').click(function(){
		$('body').removeClass('modal-open');
		$('.modal-backdrop').hide();
		$('.modal').hide();
		return false;
	});
	$('.modal').click(function(){
		if ($(this).attr('aria-hidden') !='false')
		{
			$('body').removeClass('modal-open');
			$('.modal-backdrop').hide();
			$('.modal').hide();
		}
		return false;
	});
	$('.modal-content').click(function(event){
		event.stopPropagation();
	});

});



