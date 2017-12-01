
function ml_100k()
    % plot figures for experiments on ml-100 data collection
    
    function  ml_100k_UserKNN(num, name)
        
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
        
        plot(P1, R1,  'co-',  ...
                P2, R2, 'gx-', ...
                P3, R3, 'ms-', ...
                P4, R4, 'bd-', ...
                P5, R5, 'k^-', ...
                P6, R6, 'rp-', ...
                'LineWidth', 2,  'markersize', 8);
            
        x = P5 + 0.01;
        y = R5 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('UserKNN on ml-100k');
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

    function  ml_100k_ItemKNN(num, name)

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

            plot(P1, R1,  'co-',  ...
                    P2, R2, 'gx-', ...
                    P3, R3, 'ms-', ...
                    P4, R4, 'bd-', ...
                    P5, R5, 'k^-', ...
                    P6, R6, 'rp-', ...
                    'LineWidth', 2,  'markersize', 8);
            
            x = P3 + 0.01;
            y = R3 + 0.01;
            for i=1:length(x)
                text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
            end

            title('ItemKNN on ml-100k');
            xlabel('Precision@N');
            ylabel('Recall@N');
            legend('K=5', 'K=10',  'K=20',  'K=40',  'K=80',  'K=160');
            grid on;
            %set(h,'ytick',y_axis_set);  % 设置刻度线
            %set(h,'yticklabel',y_axis_set); %设置显示哪些刻度值
            set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
            %set(get(gca,'XLabel'),'FontSize',18);  % 设置x轴标签字体大小
            %set(get(gca,'YLabel'), 'FontSize',18);  % 设置y轴标签字体大小
            %print(h, strcat('img\', name, '_ItemKNN'), '-depsc');
            print(h, strcat('img\', name, '_ItemKNN'), '-dpdf');

     end
 
   clc;
   clear;
 
    % UserKNN
    num = xlsread('ml-100k',  1, 'B3:F44'); % u1
    ml_100k_UserKNN(num, 'ml_100_u1');
    
    num = xlsread('ml-100k',  1, 'I3:M44'); % u2
    ml_100k_UserKNN(num, 'ml_100_u2');
    
    num = xlsread('ml-100k',  1, 'P3:T44'); % u3
    ml_100k_UserKNN(num, 'ml_100_u3');
    
    num = xlsread('ml-100k',  1, 'W3:AA44'); % u4
    ml_100k_UserKNN(num, 'ml_100_u4');
    
    num = xlsread('ml-100k',  1, 'AD3:AH44'); % u5
    ml_100k_UserKNN(num, 'ml_100_u5');
    
    % ItemKNN
    num = xlsread('ml-100k',  2, 'B3:F44'); % u1
    ml_100k_ItemKNN(num, 'ml_100_u1');
    
    num = xlsread('ml-100k',  2, 'I3:M44'); % u2
    ml_100k_ItemKNN(num, 'ml_100_u2');
    
    num = xlsread('ml-100k',  2, 'P3:T44'); % u3
    ml_100k_ItemKNN(num, 'ml_100_u3');
    
    num = xlsread('ml-100k',  2, 'W3:AA44'); % u4
    ml_100k_ItemKNN(num, 'ml_100_u4');
    
    num = xlsread('ml-100k',  2, 'AD3:AH44'); % u5
    ml_100k_ItemKNN(num, 'ml_100_u5');
    
    
end



