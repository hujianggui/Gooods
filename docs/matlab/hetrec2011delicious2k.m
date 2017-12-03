
function hetrec2011delicious2k()
    % plot figures for experiments on ml-100 data collection
    
    function  hetrec2011delicious2k_UserKNN(num, name)
        %% plots for userknn on ml-100k
        h = figure('Name', name);
        N  = num(1:7, 1);          % # of recommendation list
        
        P1 = num(1:7, 2);          % precision
        R1 = num(1:7, 3);          %  recall
        
        P2 = num(8:14, 2);          % precision
        R2 = num(8:14, 3);          %  recall
        
        P3 = num(15:21, 2);          % precision
        R3 = num(15:21, 3);          %  recall
        
        P4 = num(22:28, 2);          % precision
        R4 = num(22:28, 3);          %  recall
        
        P5 = num(29:35, 2);          % precision
        R5 = num(29:35, 3);          %  recall
        
        P6 = num(36:42, 2);          % precision
        R6 = num(36:42, 3);          %  recall
        
        plot(P1, R1,  'bd-',  ...
                P2, R2, 'go-', ...
                P3, R3, 'rx-', ...
                P4, R4, 'c+-', ...
                P5, R5, 'ms-', ...
                P6, R6, 'yv-', ...
                'LineWidth', 2,  'markersize', 8);
            
        x = P5 + 0.01;
        y = R5 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('UserKNN on delicious2k');
        xlabel('Precision@N');
        ylabel('Recall@N');
        legend('K=5', 'K=10',  'K=20',  'K=40',  'K=80',  'K=160');
        grid on;
        %set(h,'ytick',y_axis_set);  % 设置刻度线
        %set(h,'yticklabel',y_axis_set); %设置显示哪些刻度值
        set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
        %set(get(gca,'XLabel'),'FontSize',18);  % 设置x轴标签字体大小
        %set(get(gca,'YLabel'), 'FontSize',18);  % 设置y轴标签字体大小
        
        %print(h, strcat('img\', name, '_UserKNN'), '-depsc');
        print(h, strcat('img\', name, '_UserKNN'), '-dpdf');
    end



    %%
    clc;
    clear;
    close all;
 
    %% UserKNN
     num = xlsread('hetrec2011delicious2k',  1, 'B3:F44'); % u1
     hetrec2011delicious2k_UserKNN(num, 'delicious2k');
    

    
   
end



